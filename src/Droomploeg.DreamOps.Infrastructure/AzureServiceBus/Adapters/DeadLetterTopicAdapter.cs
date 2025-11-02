using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Extensions;
using Droomploeg.DreamOps.Infrastructure.Contexts;
using Microsoft.Extensions.Azure;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Adapters;

public class DeadLetterTopicAdapter : IDeadLetterTopicAdapter<ServiceBusMessage, ServiceBusReceivedMessage>
{
    private readonly ApplicationContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IAzureClientFactory<ServiceBusAdministrationClient> _adminClientFactory;
    private readonly IAzureClientFactory<ServiceBusClient> _clientFactory;

    public DeadLetterTopicAdapter(
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

    public async Task<ICollection<ServiceBusReceivedMessage>> PeekMessagesAsync(string topic, string subscription,
        long fromSequenceNumber = EntityAdapterConstants.DefaultStartIndex,
        int numberOfMessages = EntityAdapterConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default)
    {
        var adminClient = _adminClientFactory.CreateClient(_context);
        var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
        var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
        if (fromSequenceNumber > azureSubscriptionRuntimeProperties.DeadLetterMessageCount)
        {
            return [];
        }

        var deadLetterSubscription = ServiceBusHelper.FormatDeadLetterPath(topic, subscription);

        var client = _clientFactory.CreateClient(_context);
        var receiver = client.CreateReceiver(deadLetterSubscription, ServiceBusConstants.PeekLockOptions);
        var messages = await receiver.PeekMessagesAsync(numberOfMessages, fromSequenceNumber, cancellationToken);
        await receiver.CloseAsync(cancellationToken);
        return [.. messages];
    }

    public async Task ResubmitAllMessagesAsync(string topic, string subscription, ResubmitOptions options, CancellationToken cancellationToken)
    {
        var timestamp = _timeProvider.GetUtcNow();

        var adminClient = _adminClientFactory.CreateClient(_context);
        var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
        var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
        var numberOfMessages = azureSubscriptionRuntimeProperties.DeadLetterMessageCount;
        if (numberOfMessages < 1)
        {
            return;
        }

        var deadLetterSubscription = ServiceBusHelper.FormatDeadLetterPath(topic, subscription);

        var client = _clientFactory.CreateClient(_context);
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

    public async Task DeleteAllMessagesAsync(string topic, string subscription, CancellationToken cancellationToken)
    {
        var timestamp = _timeProvider.GetUtcNow();

        var adminClient = _adminClientFactory.CreateClient(_context);
        var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
        var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
        var numberOfMessages = azureSubscriptionRuntimeProperties.DeadLetterMessageCount;
        if (numberOfMessages < 1)
        {
            return;
        }

        var deadLetterSubscription = ServiceBusHelper.FormatDeadLetterPath(topic, subscription);

        var client = _clientFactory.CreateClient(_context);
        var receiver = client.CreateReceiver(deadLetterSubscription, ServiceBusConstants.PeekLockOptions);
        await receiver.CompleteMessagesAsync(timestamp, cancellationToken);
        await receiver.CloseAsync(cancellationToken);
    }

    public async Task<bool> ResubmitMessageAsync(string topic, string subscription, ServiceBusReceivedMessage receivedMessage, ServiceBusMessage sendMessage, ResubmitOptions options, CancellationToken cancellationToken)
    {
        var adminClient = _adminClientFactory.CreateClient(_context);
        var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
        var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
        var numberOfMessages = azureSubscriptionRuntimeProperties.DeadLetterMessageCount;
        if (numberOfMessages < 1)
        {
            return false;
        }

        var deadLetterSubscription = ServiceBusHelper.FormatDeadLetterPath(topic, subscription);

        var client = _clientFactory.CreateClient(_context);
        var receiver = client.CreateReceiver(deadLetterSubscription, ServiceBusConstants.PeekLockOptions);
        var sender = client.CreateSender(topic);

        var result = await receiver.SearchAndResubmitAsync(sender, receivedMessage, sendMessage, numberOfMessages, options, cancellationToken);
        await receiver.CloseAsync(cancellationToken);
        await sender.CloseAsync(cancellationToken);
        return result;
    }

    public async Task<bool> DeleteMessageAsync(string topic, string subscription, ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        var adminClient = _adminClientFactory.CreateClient(_context);
        var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
        var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
        var numberOfMessages = azureSubscriptionRuntimeProperties.DeadLetterMessageCount;
        var numberOfMessagesToReceive = Math.Min(numberOfMessages, message.SequenceNumber);
        if (numberOfMessagesToReceive < 1)
        {
            return false;
        }

        var deadLetterSubscription = ServiceBusHelper.FormatDeadLetterPath(topic, subscription);
        var client = _clientFactory.CreateClient(_context);
        var receiver = client.CreateReceiver(deadLetterSubscription, ServiceBusConstants.PeekLockOptions);
        var result = await receiver.SearchAndCompleteAsync(message, numberOfMessagesToReceive, cancellationToken);
        await receiver.CloseAsync(cancellationToken);
        return result;
    }
}
