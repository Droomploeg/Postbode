using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Extensions;
using Droomploeg.DreamOps.Infrastructure.Contexts;
using Microsoft.Extensions.Azure;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Adapters;

public class ActiveTopicAdapter : IActiveTopicAdapter<ServiceBusMessage, ServiceBusReceivedMessage>
{
    private readonly ApplicationContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IAzureClientFactory<ServiceBusAdministrationClient> _adminClientFactory;
    private readonly IAzureClientFactory<ServiceBusClient> _clientFactory;

    public ActiveTopicAdapter(
        TimeProvider timeProvider,
        ApplicationContext context,
        IAzureClientFactory<ServiceBusAdministrationClient> adminClientFactory,
        IAzureClientFactory<ServiceBusClient> clientFactory)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _adminClientFactory = adminClientFactory ?? throw new ArgumentNullException(nameof(adminClientFactory));
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<bool> SendAsync(string topic, ServiceBusMessage messages, CancellationToken cancellationToken = default)
    {
        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.UserAccount);
        var sender = client
            .CreateSender(topic);

        await sender.SendBulkMessageAsync([messages], cancellationToken);
        return true;
    }

    public async Task<ICollection<ServiceBusReceivedMessage>> PeekMessagesAsync(string topic, string subscription,
        long fromSequenceNumber = EntityAdapterConstants.DefaultStartIndex,
        int numberOfMessages = EntityAdapterConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default)
    {

        var adminClient = _adminClientFactory.CreateClient(_context);
        var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
        var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
        if (fromSequenceNumber > azureSubscriptionRuntimeProperties.ActiveMessageCount)
        {
            return [];
        }

        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.UserAccount);
        var receiver = client.CreateReceiver(topic, subscription, ServiceBusConstants.PeekLockOptions);
        var messages = await receiver.PeekMessagesAsync(numberOfMessages, fromSequenceNumber, cancellationToken);
        await receiver.CloseAsync(cancellationToken);

        return [.. messages];
    }

    public async Task DeleteAllMessagesAsync(string topic, string subscription, CancellationToken cancellationToken)
    {
        var timestamp = _timeProvider.GetUtcNow();

        var adminClient = _adminClientFactory.CreateClient(_context);
        var azureSubscriptionResponse = await adminClient.GetSubscriptionAsync(topic, subscription, cancellationToken);
        var azureSubscription = azureSubscriptionResponse.Value;

        var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
        var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
        if (azureSubscriptionRuntimeProperties.ActiveMessageCount < 1)
        {
            return;
        }

        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.ServiceAccount);
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
        var adminClient = _adminClientFactory.CreateClient(_context);
        var azureSubscriptionResponse = await adminClient.GetSubscriptionAsync(topic, subscription, cancellationToken);
        var azureSubscription = azureSubscriptionResponse.Value;

        var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
        var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
        if (azureSubscriptionRuntimeProperties.ActiveMessageCount < 1)
        {
            return false;
        }

        var result = false;
        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.UserAccount);
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

    public async Task<bool> DeadLetterMessageAsync(string topic, string subscription, ServiceBusReceivedMessage message, string source, string reason, string description,
        CancellationToken cancellationToken)
    {
        var adminClient = _adminClientFactory.CreateClient(_context);
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

        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.UserAccount);
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
