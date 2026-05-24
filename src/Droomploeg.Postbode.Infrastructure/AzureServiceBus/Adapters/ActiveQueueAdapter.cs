using Azure.Messaging.ServiceBus;
using Droomploeg.Postbode.Application.ServiceBus.Adapters;
using Droomploeg.Postbode.Domain.ServiceBus.Types;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Extensions;
using Droomploeg.Postbode.Infrastructure.Contexts;
using Microsoft.Extensions.Azure;

namespace Droomploeg.Postbode.Infrastructure.AzureServiceBus.Adapters;

/// <summary>
/// Adapter for active queue operations using Azure Service Bus.
/// Handles sending, peeking, deleting, and dead-lettering messages on active queues.
/// </summary>
public class ActiveQueueAdapter : IActiveQueueAdapter<ServiceBusMessage, ServiceBusReceivedMessage>
{
    private readonly TimeProvider _timeProvider;
    private readonly ApplicationContext _context;
    private readonly ISessionInfoProvider _sessionInfoProvider;
    private readonly IAzureClientFactory<ServiceBusClient> _clientFactory;

    /// <summary>
    /// Initializes a new instance of <see cref="ActiveQueueAdapter"/>.
    /// </summary>
    /// <param name="timeProvider">Provider for retrieving the current time.</param>
    /// <param name="context"><see cref="ApplicationContext"/> for the current request.</param>
    /// <param name="sessionInfoProvider">Provider for determining whether a queue requires sessions.</param>
    /// <param name="clientFactory">Factory for creating <see cref="ServiceBusClient"/> instances.</param>
    public ActiveQueueAdapter(
        TimeProvider timeProvider,
        ApplicationContext context,
        ISessionInfoProvider sessionInfoProvider,
        IAzureClientFactory<ServiceBusClient> clientFactory)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _sessionInfoProvider = sessionInfoProvider ?? throw new ArgumentNullException(nameof(sessionInfoProvider));
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
    }

    /// <inheritdoc />
    public async Task<bool> SendAsync(string queue, ServiceBusMessage message, CancellationToken cancellationToken = default)
    {
        message.CorrelationId = _context.CorrelationId.ToString();
        message.ApplicationProperties["UserName"] = _context.UserName;

        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.UserAccount);
        var sender = client.CreateSender(queue);

        await sender.SendBulkMessageAsync([message], cancellationToken);
        await sender.CloseAsync(cancellationToken);

        return true;
    }

    /// <inheritdoc />
    public async Task<ICollection<ServiceBusReceivedMessage>> PeekMessagesAsync(string queue,
        long fromSequenceNumber = EntityAdapterConstants.DefaultStartIndex,
        int numberOfMessages = EntityAdapterConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default)
    {
        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.UserAccount);
        var receiver = client.CreateReceiver(queue, ServiceBusConstants.PeekLockOptions);
        var messages = await receiver.PeekMessagesAsync(numberOfMessages, fromSequenceNumber, cancellationToken);
        await receiver.CloseAsync(cancellationToken);

        return [.. messages];
    }

    /// <inheritdoc />
    public async Task DeleteAllMessagesAsync(string queue, CancellationToken cancellationToken)
    {
        var timestamp = _timeProvider.GetUtcNow();
        var requiresSession = await _sessionInfoProvider.RequiresSessionAsync(queue, cancellationToken);

        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.ServiceAccount);
        if (requiresSession)
        {
            var receiver = await client.AcceptNextSessionAsync(queue, ServiceBusConstants.PeekLockSessionOptions, cancellationToken: cancellationToken);
            await receiver.CompleteMessagesAsync(timestamp, cancellationToken);
            await receiver.CloseAsync(cancellationToken);
        }
        else
        {
            var receiver = client.CreateReceiver(queue, ServiceBusConstants.PeekLockOptions);
            await receiver.CompleteMessagesAsync(timestamp, cancellationToken);
            await receiver.CloseAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteMessageAsync(string queue, ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        var requiresSession = await _sessionInfoProvider.RequiresSessionAsync(queue, cancellationToken);

        var result = false;
        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.UserAccount);
        if (requiresSession)
        {
            var receiver = await client.AcceptNextSessionAsync(queue, ServiceBusConstants.PeekLockSessionOptions, cancellationToken: cancellationToken);
            var queueMessage = await receiver.ReceiveMessageAsync(cancellationToken: cancellationToken);
            if (message.Compare(queueMessage))
            {
                await receiver.CompleteMessageAsync(queueMessage, cancellationToken: cancellationToken);
                result = true;
            }
            await receiver.CloseAsync(cancellationToken);
        }
        else
        {
            var receiver = client.CreateReceiver(queue, ServiceBusConstants.PeekLockOptions);
            var queueMessage = await receiver.ReceiveMessageAsync(cancellationToken: cancellationToken);
            if (message.Compare(queueMessage))
            {
                await receiver.CompleteMessageAsync(queueMessage, cancellationToken: cancellationToken);
                result = true;
            }
            await receiver.CloseAsync(cancellationToken);
        }
        return result;
    }

    /// <inheritdoc />
    public async Task<bool> DeadLetterMessageAsync(string queue, ServiceBusReceivedMessage message, string source, string reason, string description,
        CancellationToken cancellationToken)
    {
        var requiresSession = await _sessionInfoProvider.RequiresSessionAsync(queue, cancellationToken);
        var dlqProperties = ServiceBusHelper.GetDeadLetterProperties(source, reason, description);

        var result = false;
        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.UserAccount);
        if (requiresSession)
        {
            var receiver = await client.AcceptNextSessionAsync(queue, ServiceBusConstants.PeekLockSessionOptions, cancellationToken: cancellationToken);
            var queueMessage = await receiver.ReceiveMessageAsync(cancellationToken: cancellationToken);
            if (message.Compare(queueMessage))
            {
                await receiver.DeadLetterMessageAsync(queueMessage, dlqProperties, cancellationToken: cancellationToken);
                result = true;
            }
            await receiver.CloseAsync(cancellationToken);
        }
        else
        {
            var receiver = client.CreateReceiver(queue, ServiceBusConstants.PeekLockOptions);
            var queueMessage = await receiver.ReceiveMessageAsync(cancellationToken: cancellationToken);
            if (message.Compare(queueMessage))
            {
                await receiver.DeadLetterMessageAsync(queueMessage, dlqProperties, cancellationToken: cancellationToken);
                result = true;
            }
            await receiver.CloseAsync(cancellationToken);
        }

        return result;
    }

}
