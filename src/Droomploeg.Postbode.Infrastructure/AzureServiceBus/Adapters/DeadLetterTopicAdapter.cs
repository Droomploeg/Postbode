using Azure.Messaging.ServiceBus;
using Droomploeg.Postbode.Application.ServiceBus.Adapters;
using Droomploeg.Postbode.Domain.ServiceBus.Models;
using Droomploeg.Postbode.Domain.ServiceBus.Types;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Extensions;
using Droomploeg.Postbode.Infrastructure.Contexts;
using Microsoft.Extensions.Azure;

namespace Droomploeg.Postbode.Infrastructure.AzureServiceBus.Adapters;

/// <summary>Adapter for dead-letter topic subscription operations using Azure Service Bus.</summary>
public class DeadLetterTopicAdapter : IDeadLetterTopicAdapter<ServiceBusMessage, ServiceBusReceivedMessage>
{
    private readonly ApplicationContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IAzureClientFactory<ServiceBusClient> _clientFactory;

    /// <summary>Initializes a new instance of <see cref="DeadLetterTopicAdapter"/>.</summary>
    /// <param name="timeProvider">Provider for retrieving the current time.</param>
    /// <param name="context"><see cref="ApplicationContext"/> for the current request.</param>
    /// <param name="clientFactory">Factory for creating <see cref="ServiceBusClient"/> instances.</param>
    public DeadLetterTopicAdapter(
        TimeProvider timeProvider,
        ApplicationContext context,
        IAzureClientFactory<ServiceBusClient> clientFactory)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<ICollection<ServiceBusReceivedMessage>> PeekMessagesAsync(string topic, string subscription,
        long fromSequenceNumber = EntityAdapterConstants.DefaultStartIndex,
        int numberOfMessages = EntityAdapterConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default)
    {
        var deadLetterSubscription = ServiceBusHelper.FormatDeadLetterPath(topic, subscription);

        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.UserAccount);
        var receiver = client.CreateReceiver(deadLetterSubscription, ServiceBusConstants.PeekLockOptions);
        var messages = await receiver.PeekMessagesAsync(numberOfMessages, fromSequenceNumber, cancellationToken);
        await receiver.CloseAsync(cancellationToken);
        return [.. messages];
    }

    /// <inheritdoc />
    public async Task ResubmitAllMessagesAsync(string topic, string subscription, ResubmitOptions options, CancellationToken cancellationToken)
    {
        var timestamp = _timeProvider.GetUtcNow();
        var deadLetterSubscription = ServiceBusHelper.FormatDeadLetterPath(topic, subscription);

        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.ServiceAccount);
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

    /// <inheritdoc />
    public async Task DeleteAllMessagesAsync(string topic, string subscription, CancellationToken cancellationToken)
    {
        var timestamp = _timeProvider.GetUtcNow();
        var deadLetterSubscription = ServiceBusHelper.FormatDeadLetterPath(topic, subscription);

        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.ServiceAccount);
        var receiver = client.CreateReceiver(deadLetterSubscription, ServiceBusConstants.PeekLockOptions);
        await receiver.CompleteMessagesAsync(timestamp, cancellationToken);
        await receiver.CloseAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ResubmitMessageAsync(string topic, string subscription, ServiceBusReceivedMessage receivedMessage, ServiceBusMessage sendMessage, ResubmitOptions options, CancellationToken cancellationToken)
    {
        sendMessage.CorrelationId = _context.CorrelationId.ToString();
        sendMessage.ApplicationProperties["UserName"] = _context.UserName;
        sendMessage.ApplicationProperties["ResubmittedAt"] = DateTimeOffset.UtcNow;

        var deadLetterSubscription = ServiceBusHelper.FormatDeadLetterPath(topic, subscription);

        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.UserAccount);
        var receiver = client.CreateReceiver(deadLetterSubscription, ServiceBusConstants.PeekLockOptions);
        var sender = client.CreateSender(topic);

        var result = await receiver.SearchAndResubmitAsync(sender, receivedMessage, sendMessage, receivedMessage.SequenceNumber, options, cancellationToken);
        await receiver.CloseAsync(cancellationToken);
        await sender.CloseAsync(cancellationToken);
        return result;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteMessageAsync(string topic, string subscription, ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        var deadLetterSubscription = ServiceBusHelper.FormatDeadLetterPath(topic, subscription);
        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.UserAccount);
        var receiver = client.CreateReceiver(deadLetterSubscription, ServiceBusConstants.PeekLockOptions);
        var result = await receiver.SearchAndCompleteAsync(message, message.SequenceNumber, cancellationToken);
        await receiver.CloseAsync(cancellationToken);
        return result;
    }
}
