using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Core;
using Droomploeg.DreamOps.Core.Models;
using Droomploeg.DreamOps.Core.Repositories;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Extensions;
using Microsoft.Extensions.Azure;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Repositories;

public class TopicRepository(
    TimeProvider timeProvider,
    IServiceBusClientContext context,
    IAzureClientFactory<ServiceBusAdministrationClient> adminClientFactory,
    IAzureClientFactory<ServiceBusClient> clientFactory) : ITopicRepository<ServiceBusMessage, ServiceBusReceivedMessage>
{
    #region [Active Queue]

    public async Task SendAsync(string topic, ICollection<ServiceBusMessage> messages, CancellationToken cancellationToken = default)
    {
        var client = clientFactory.CreateClient(context.CurrentClient);
        var sender = client
            .CreateSender(topic);

        await sender.SendBulkMessageAsync(messages, cancellationToken);
    }

    public async Task<IEnumerable<ServiceBusReceivedMessage>> PeekActiveMessagesAsync(string topic, string subscription,
        long fromSequenceNumber = RepositoryConstants.DefaultStartIndex,
        int numberOfMessages = RepositoryConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default)
    {
        var adminClient = adminClientFactory.CreateClient(context.CurrentClient);
        var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
        var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
        if (fromSequenceNumber > azureSubscriptionRuntimeProperties.ActiveMessageCount)
        {
            return [];
        }

        var client = clientFactory.CreateClient(context.CurrentClient);
        var receiver = client.CreateReceiver(topic, subscription, ServiceBusConstants.PeekLockOptions);
        var messages = await receiver.PeekMessagesAsync(numberOfMessages, fromSequenceNumber, cancellationToken);
        await receiver.CloseAsync(cancellationToken);

        return messages;
    }

    public async Task DeleteAllActiveMessagesAsync(string topic, string subscription, CancellationToken cancellationToken)
    {
        var timestamp = timeProvider.GetUtcNow();

        var adminClient = adminClientFactory.CreateClient(context.CurrentClient);
        var azureSubscriptionResponse = await adminClient.GetSubscriptionAsync(topic, subscription, cancellationToken);
        var azureSubscription = azureSubscriptionResponse.Value;

        var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
        var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
        if (azureSubscriptionRuntimeProperties.ActiveMessageCount < 1)
        {
            return;
        }

        var client = clientFactory.CreateClient(context.CurrentClient);
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

    public async Task<bool> DeleteActiveMessageAsync(string topic, string subscription, ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        var adminClient = adminClientFactory.CreateClient(context.CurrentClient);
        var azureSubscriptionResponse = await adminClient.GetSubscriptionAsync(topic, subscription, cancellationToken);
        var azureSubscription = azureSubscriptionResponse.Value;

        var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
        var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
        if (azureSubscriptionRuntimeProperties.ActiveMessageCount < 1)
        {
            return false;
        }

        var result = false;
        var client = clientFactory.CreateClient(context.CurrentClient);
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
        var adminClient = adminClientFactory.CreateClient(context.CurrentClient);
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

        var client = clientFactory.CreateClient(context.CurrentClient);
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

    #endregion [Active Queue]

    #region [Dead Letter Queue]
    public async Task<IEnumerable<ServiceBusReceivedMessage>> PeekDeadLetterMessagesAsync(string topic, string subscription,
        long fromSequenceNumber = RepositoryConstants.DefaultStartIndex,
        int numberOfMessages = RepositoryConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default)
    {
        var adminClient = adminClientFactory.CreateClient(context.CurrentClient);
        var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
        var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
        if (fromSequenceNumber > azureSubscriptionRuntimeProperties.DeadLetterMessageCount)
        {
            return [];
        }

        var deadLetterSubscription = ServiceBusHelper.FormatDeadLetterPath(topic, subscription);

        var client = clientFactory.CreateClient(context.CurrentClient);
        var receiver = client.CreateReceiver(deadLetterSubscription, ServiceBusConstants.PeekLockOptions);
        var messages = await receiver.PeekMessagesAsync(numberOfMessages, fromSequenceNumber, cancellationToken);
        await receiver.CloseAsync(cancellationToken);
        return messages;
    }

    public async Task ResubmitAllDeadLetterMessagesAsync(string topic, string subscription, ResubmitOptions options, CancellationToken cancellationToken)
    {
        var timestamp = timeProvider.GetUtcNow();

        var adminClient = adminClientFactory.CreateClient(context.CurrentClient);
        var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
        var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
        var numberOfMessages = azureSubscriptionRuntimeProperties.DeadLetterMessageCount;
        if (numberOfMessages < 1)
        {
            return;
        }

        var deadLetterSubscription = ServiceBusHelper.FormatDeadLetterPath(topic, subscription);

        var client = clientFactory.CreateClient(context.CurrentClient);
        var receiver = client.CreateReceiver(deadLetterSubscription, ServiceBusConstants.PeekLockOptions);
        var sender = client.CreateSender(topic);
        if (options.DeleteMessage)
        {
            await receiver.ResubmitNumberOfMessagesWithDeleteAsync(sender, timestamp, options.GenerateMessageIds, cancellationToken);
        }
        else
        {
            await receiver.ResubmitNumberOfMessagesAsync(sender, timestamp, options.GenerateMessageIds, cancellationToken);
        }

        await receiver.CloseAsync(cancellationToken);
        await sender.CloseAsync(cancellationToken);
    }

    public async Task DeleteAllDeadLetterMessagesAsync(string topic, string subscription, CancellationToken cancellationToken)
    {
        var timestamp = timeProvider.GetUtcNow();

        var adminClient = adminClientFactory.CreateClient(context.CurrentClient);
        var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
        var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
        var numberOfMessages = azureSubscriptionRuntimeProperties.DeadLetterMessageCount;
        if (numberOfMessages < 1)
        {
            return;
        }

        var deadLetterSubscription = ServiceBusHelper.FormatDeadLetterPath(topic, subscription);

        var client = clientFactory.CreateClient(context.CurrentClient);
        var receiver = client.CreateReceiver(deadLetterSubscription, ServiceBusConstants.PeekLockOptions);
        await receiver.CompleteMessagesAsync(timestamp, cancellationToken);
        await receiver.CloseAsync(cancellationToken);
    }

    public async Task<bool> ResubmitDeadLetterMessageAsync(string topic, string subscription, ServiceBusReceivedMessage receivedMessage, ServiceBusMessage sendMessage, ResubmitOptions options, CancellationToken cancellationToken)
    {
        var adminClient = adminClientFactory.CreateClient(context.CurrentClient);
        var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
        var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
        var numberOfMessages = azureSubscriptionRuntimeProperties.DeadLetterMessageCount;
        if (numberOfMessages < 1)
        {
            return false;
        }

        var deadLetterSubscription = ServiceBusHelper.FormatDeadLetterPath(topic, subscription);

        var client = clientFactory.CreateClient(context.CurrentClient);
        var receiver = client.CreateReceiver(deadLetterSubscription, ServiceBusConstants.PeekLockOptions);
        var sender = client.CreateSender(topic);

        var result = await receiver.SearchAndResubmitAsync(sender, receivedMessage, sendMessage, numberOfMessages, options, cancellationToken);
        await receiver.CloseAsync(cancellationToken);
        await sender.CloseAsync(cancellationToken);
        return result;
    }

    public async Task<bool> DeleteDeadLetterMessageAsync(string topic, string subscription,ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        var adminClient = adminClientFactory.CreateClient(context.CurrentClient);
        var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
        var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
        var numberOfMessages = azureSubscriptionRuntimeProperties.DeadLetterMessageCount;
        var numberOfMessagesToReceive = Math.Min(numberOfMessages, message.SequenceNumber);
        if (numberOfMessagesToReceive < 1)
        {
            return false;
        }

        var deadLetterSubscription = ServiceBusHelper.FormatDeadLetterPath(topic, subscription);
        var client = clientFactory.CreateClient(context.CurrentClient);
        var receiver = client.CreateReceiver(deadLetterSubscription, ServiceBusConstants.PeekLockOptions);
        var result = await receiver.SearchAndCompleteAsync(message, numberOfMessagesToReceive, cancellationToken);
        await receiver.CloseAsync(cancellationToken);
        return result;
    }

    #endregion [Dead Letter Queue]
}
