using Azure.Messaging.ServiceBus;
using Droomploeg.Postbode.Application.ServiceBus.Adapters;
using Droomploeg.Postbode.Domain.ServiceBus.Models;
using Droomploeg.Postbode.Domain.ServiceBus.Types;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Extensions;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Factories;
using Droomploeg.Postbode.Infrastructure.Contexts;

namespace Droomploeg.Postbode.Infrastructure.AzureServiceBus.Adapters;

/// <summary>Adapter for dead-letter queue operations using Azure Service Bus.</summary>
public class DeadLetterQueueAdapter : IDeadLetterQueueAdapter<ServiceBusMessage, ServiceBusReceivedMessage>
{
    private readonly ApplicationContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IUserServiceBusClientProvider _userClientProvider;

    /// <summary>Initializes a new instance of <see cref="DeadLetterQueueAdapter"/>.</summary>
    /// <param name="timeProvider">Provider for retrieving the current time.</param>
    /// <param name="context"><see cref="ApplicationContext"/> for the current request.</param>
    /// <param name="userClientProvider">Provider for the <see cref="ServiceBusClient"/> of the signed-in user.</param>
    public DeadLetterQueueAdapter(
        TimeProvider timeProvider,
        ApplicationContext context,
        IUserServiceBusClientProvider userClientProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _userClientProvider = userClientProvider ?? throw new ArgumentNullException(nameof(userClientProvider));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<ICollection<ServiceBusReceivedMessage>> PeekMessagesAsync(string queue,
        long fromSequenceNumber = EntityAdapterConstants.DefaultStartIndex,
        int numberOfMessages = EntityAdapterConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default)
    {
        var deadLetterQueueName = ServiceBusHelper.FormatDeadLetterPath(queue);

        var client = await _userClientProvider.GetClientAsync(_context.CurrentConnection, cancellationToken);
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

        var client = await _userClientProvider.GetClientAsync(_context.CurrentConnection, cancellationToken);
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

        var client = await _userClientProvider.GetClientAsync(_context.CurrentConnection, cancellationToken);
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

        var client = await _userClientProvider.GetClientAsync(_context.CurrentConnection, cancellationToken);
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

        var client = await _userClientProvider.GetClientAsync(_context.CurrentConnection, cancellationToken);
        var receiver = client.CreateReceiver(deadLetterQueueName, ServiceBusConstants.PeekLockOptions);
        var result = await receiver.SearchAndCompleteAsync(message, message.SequenceNumber, cancellationToken);

        await receiver.CloseAsync(cancellationToken);

        return result;
    }
}
