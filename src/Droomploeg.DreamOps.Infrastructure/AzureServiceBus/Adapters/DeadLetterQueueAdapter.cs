using Azure.Messaging.ServiceBus;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Extensions;
using Droomploeg.DreamOps.Infrastructure.Contexts;
using Microsoft.Extensions.Azure;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Adapters;

/// <summary>Adapter for dead-letter queue operations using Azure Service Bus.</summary>
public class DeadLetterQueueAdapter : IDeadLetterQueueAdapter<ServiceBusMessage, ServiceBusReceivedMessage>
{
    private readonly ApplicationContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IAzureClientFactory<ServiceBusClient> _clientFactory;

    /// <summary>Initializes a new instance of <see cref="DeadLetterQueueAdapter"/>.</summary>
    /// <param name="timeProvider">Provider for retrieving the current time.</param>
    /// <param name="context"><see cref="ApplicationContext"/> for the current request.</param>
    /// <param name="clientFactory">Factory for creating <see cref="ServiceBusClient"/> instances.</param>
    public DeadLetterQueueAdapter(
        TimeProvider timeProvider,
        ApplicationContext context,
        IAzureClientFactory<ServiceBusClient> clientFactory)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<ICollection<ServiceBusReceivedMessage>> PeekMessagesAsync(string queue,
        long fromSequenceNumber = EntityAdapterConstants.DefaultStartIndex,
        int numberOfMessages = EntityAdapterConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default)
    {
        var deadLetterQueueName = ServiceBusHelper.FormatDeadLetterPath(queue);

        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.UserAccount);
        var receiver = client.CreateReceiver(deadLetterQueueName, ServiceBusConstants.PeekLockOptions);
        var messages = await receiver.PeekMessagesAsync(numberOfMessages, fromSequenceNumber, cancellationToken);
        await receiver.CloseAsync(cancellationToken);

        return [.. messages];
    }

    /// <inheritdoc />
    public async Task ResubmitAllMessagesAsync(string queue, ResubmitOptions options, CancellationToken cancellationToken)
    {
        var timestamp = _timeProvider.GetUtcNow();
        var deadLetterQueueName = ServiceBusHelper.FormatDeadLetterPath(queue);

        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.ServiceAccount);
        var receiver = client.CreateReceiver(deadLetterQueueName, ServiceBusConstants.PeekLockOptions);
        var sender = client.CreateSender(queue);
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
    public async Task DeleteAllMessagesAsync(string queue, CancellationToken cancellationToken)
    {
        var timestamp = _timeProvider.GetUtcNow();
        var deadLetterQueueName = ServiceBusHelper.FormatDeadLetterPath(queue);

        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.ServiceAccount);
        var receiver = client.CreateReceiver(deadLetterQueueName, ServiceBusConstants.PeekLockOptions);
        await receiver.CompleteMessagesAsync(timestamp, cancellationToken);
        await receiver.CloseAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ResubmitMessageAsync(string queue, ServiceBusReceivedMessage receivedMessage, ServiceBusMessage sendMessage, ResubmitOptions options, CancellationToken cancellationToken)
    {
        sendMessage.CorrelationId = _context.CorrelationId.ToString();
        sendMessage.ApplicationProperties["UserName"] = _context.UserName;
        sendMessage.ApplicationProperties["ResubmittedAt"] = DateTimeOffset.UtcNow;

        var deadLetterQueueName = ServiceBusHelper.FormatDeadLetterPath(queue);

        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.UserAccount);
        var receiver = client.CreateReceiver(deadLetterQueueName, ServiceBusConstants.PeekLockOptions);
        var sender = client.CreateSender(queue);

        var result = await receiver.SearchAndResubmitAsync(sender, receivedMessage, sendMessage, receivedMessage.SequenceNumber, options, cancellationToken);

        await receiver.CloseAsync(cancellationToken);
        await sender.CloseAsync(cancellationToken);

        return result;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteMessageAsync(string queue,
        ServiceBusReceivedMessage message,
        CancellationToken cancellationToken)
    {
        var deadLetterQueueName = ServiceBusHelper.FormatDeadLetterPath(queue);

        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.UserAccount);
        var receiver = client.CreateReceiver(deadLetterQueueName, ServiceBusConstants.PeekLockOptions);
        var result = await receiver.SearchAndCompleteAsync(message, message.SequenceNumber, cancellationToken);

        await receiver.CloseAsync(cancellationToken);

        return result;
    }
}
