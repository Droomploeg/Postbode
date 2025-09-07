using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Core;
using Droomploeg.DreamOps.Core.Repositories;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Extensions;
using Microsoft.Extensions.Azure;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Repositories;

public class ActiveTopicRepository(
    TimeProvider timeProvider,
    IServiceBusInfoContext context,
    IAzureClientFactory<ServiceBusAdministrationClient> adminClientFactory,
    IAzureClientFactory<ServiceBusClient> clientFactory) : IActiveTopicRepository<ServiceBusMessage, ServiceBusReceivedMessage>
{
    public async Task SendAsync(string topic, ICollection<ServiceBusMessage> messages, CancellationToken cancellationToken = default)
    {
        var client = clientFactory.CreateClient(context.Current.Name);
        var sender = client
            .CreateSender(topic);

        await sender.SendBulkMessageAsync(messages, cancellationToken);
    }

    public async Task<IEnumerable<ServiceBusReceivedMessage>> PeekMessagesAsync(string topic, string subscription,
        long fromSequenceNumber = RepositoryConstants.DefaultStartIndex,
        int numberOfMessages = RepositoryConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default)
    {
        var adminClient = adminClientFactory.CreateClient(context.Current.Name);
        var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
        var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
        if (fromSequenceNumber > azureSubscriptionRuntimeProperties.ActiveMessageCount)
        {
            return [];
        }

        var client = clientFactory.CreateClient(context.Current.Name);
        var receiver = client.CreateReceiver(topic, subscription, ServiceBusConstants.PeekLockOptions);
        var messages = await receiver.PeekMessagesAsync(numberOfMessages, fromSequenceNumber, cancellationToken);
        await receiver.CloseAsync(cancellationToken);

        return messages;
    }

    public async Task DeleteAllMessagesAsync(string topic, string subscription, CancellationToken cancellationToken)
    {
        var timestamp = timeProvider.GetUtcNow();

        var adminClient = adminClientFactory.CreateClient(context.Current.Name);
        var azureSubscriptionResponse = await adminClient.GetSubscriptionAsync(topic, subscription, cancellationToken);
        var azureSubscription = azureSubscriptionResponse.Value;

        var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
        var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
        if (azureSubscriptionRuntimeProperties.ActiveMessageCount < 1)
        {
            return;
        }

        var client = clientFactory.CreateClient(context.Current.Name);
        if (azureSubscription.RequiresSession)
        {
            var receiver = await client.AcceptNextSessionAsync(topic, subscription, ServiceBusConstants.PeekLockSessionOptions, cancellationToken: cancellationToken);
            await receiver.CompleteMessagesAsync(timestamp, cancellationToken);
            await receiver.CloseAsync(cancellationToken);
        }
        else
        {
            var receiver = client.CreateReceiver(topic, subscription, ServiceBusConstants.PeekLockOptions);
            await receiver.CompleteMessagesAsync(timestamp, cancellationToken);
            await receiver.CloseAsync(cancellationToken);
        }
    }

    public async Task<bool> DeleteMessageAsync(string topic, string subscription, ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        var adminClient = adminClientFactory.CreateClient(context.Current.Name);
        var azureSubscriptionResponse = await adminClient.GetSubscriptionAsync(topic, subscription, cancellationToken);
        var azureSubscription = azureSubscriptionResponse.Value;

        var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
        var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
        if (azureSubscriptionRuntimeProperties.ActiveMessageCount < 1)
        {
            return false;
        }

        var result = false;
        var client = clientFactory.CreateClient(context.Current.Name);
        if (azureSubscription.RequiresSession)
        {
            var receiver = await client.AcceptNextSessionAsync(topic, subscription, ServiceBusConstants.PeekLockSessionOptions, cancellationToken: cancellationToken);
            var subscriptionMessage = await receiver.ReceiveMessageAsync(cancellationToken: cancellationToken);
            if (message.Compare(subscriptionMessage))
            {
                await receiver.CompleteMessageAsync(subscriptionMessage, cancellationToken: cancellationToken);
                result = true;
            }
            await receiver.CloseAsync(cancellationToken);
        }
        else
        {
            var receiver = client.CreateReceiver(topic, subscription, ServiceBusConstants.PeekLockOptions);
            var subscriptionMessage = await receiver.ReceiveMessageAsync(cancellationToken: cancellationToken);
            if (message.Compare(subscriptionMessage))
            {
                await receiver.CompleteMessageAsync(subscriptionMessage, cancellationToken: cancellationToken);
                result = true;
            }
            await receiver.CloseAsync(cancellationToken);
        }
        return result;
    }

    public async Task<bool> DeadLetterMessagesAsync(string topic, string subscription, ServiceBusReceivedMessage message, string source, string reason, string description,
        CancellationToken cancellationToken)
    {
        var adminClient = adminClientFactory.CreateClient(context.Current.Name);
        var azureSubscriptionResponse = await adminClient.GetSubscriptionAsync(topic, subscription, cancellationToken);
        var azureSubscription = azureSubscriptionResponse.Value;

        var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
        var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
        if (azureSubscriptionRuntimeProperties.ActiveMessageCount < 1)
        {
            return false;
        }

        var result = false;
        var dlqProperties = ServiceBusHelper.GetDeadLetterProperties(source, reason, description);

        var client = clientFactory.CreateClient(context.Current.Name);
        if (azureSubscription.RequiresSession)
        {
            var receiver = await client.AcceptNextSessionAsync(topic, subscription, ServiceBusConstants.PeekLockSessionOptions, cancellationToken: cancellationToken);
            var subscriptionMessage = await receiver.ReceiveMessageAsync(cancellationToken: cancellationToken);
            if (message.Compare(subscriptionMessage))
            {
                await receiver.DeadLetterMessageAsync(subscriptionMessage, dlqProperties, cancellationToken: cancellationToken);
                result = true;
            }
            await receiver.CloseAsync(cancellationToken);
        }
        else
        {
            var receiver = client.CreateReceiver(topic, subscription, ServiceBusConstants.PeekLockOptions);
            var subscriptionMessage = await receiver.ReceiveMessageAsync(cancellationToken: cancellationToken);
            if (message.Compare(subscriptionMessage))
            {
                await receiver.DeadLetterMessageAsync(subscriptionMessage, dlqProperties, cancellationToken: cancellationToken);
                result = true;
            }

            await receiver.CloseAsync(cancellationToken);
        }

        return result;
    }
}
