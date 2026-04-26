using Azure.Messaging.ServiceBus;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Extensions;
using Droomploeg.DreamOps.Infrastructure.Contexts;
using Microsoft.Extensions.Azure;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Adapters;

/// <summary>
/// Adapter for active topic subscription operations using Azure Service Bus.
/// Handles sending, peeking, deleting, and dead-lettering messages on topic subscriptions.
/// </summary>
public class ActiveTopicAdapter : IActiveTopicAdapter<ServiceBusMessage, ServiceBusReceivedMessage>
{
    private readonly ApplicationContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ISessionInfoProvider _sessionInfoProvider;
    private readonly IAzureClientFactory<ServiceBusClient> _clientFactory;

    /// <summary>
    /// Initializes a new instance of <see cref="ActiveTopicAdapter"/>.
    /// </summary>
    /// <param name="timeProvider">Provider for retrieving the current time.</param>
    /// <param name="context"><see cref="ApplicationContext"/> for the current request.</param>
    /// <param name="sessionInfoProvider">Provider for determining whether a subscription requires sessions.</param>
    /// <param name="clientFactory">Factory for creating <see cref="ServiceBusClient"/> instances.</param>
    public ActiveTopicAdapter(
        TimeProvider timeProvider,
        ApplicationContext context,
        ISessionInfoProvider sessionInfoProvider,
        IAzureClientFactory<ServiceBusClient> clientFactory)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _sessionInfoProvider = sessionInfoProvider ?? throw new ArgumentNullException(nameof(sessionInfoProvider));
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<bool> SendAsync(string topic, ServiceBusMessage messages, CancellationToken cancellationToken = default)
    {
        messages.CorrelationId = _context.CorrelationId.ToString();
        messages.ApplicationProperties["UserName"] = _context.UserName;

        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.UserAccount);
        var sender = client.CreateSender(topic);

        await sender.SendBulkMessageAsync([messages], cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public async Task<ICollection<ServiceBusReceivedMessage>> PeekMessagesAsync(string topic, string subscription,
        long fromSequenceNumber = EntityAdapterConstants.DefaultStartIndex,
        int numberOfMessages = EntityAdapterConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default)
    {
        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.UserAccount);
        var receiver = client.CreateReceiver(topic, subscription, ServiceBusConstants.PeekLockOptions);
        var messages = await receiver.PeekMessagesAsync(numberOfMessages, fromSequenceNumber, cancellationToken);
        await receiver.CloseAsync(cancellationToken);

        return [.. messages];
    }

    /// <inheritdoc />
    public async Task DeleteAllMessagesAsync(string topic, string subscription, CancellationToken cancellationToken)
    {
        var timestamp = _timeProvider.GetUtcNow();
        var requiresSession = await _sessionInfoProvider.RequiresSessionAsync(topic, subscription, cancellationToken);

        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.ServiceAccount);
        if (requiresSession)
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

    /// <inheritdoc />
    public async Task<bool> DeleteMessageAsync(string topic, string subscription, ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        var requiresSession = await _sessionInfoProvider.RequiresSessionAsync(topic, subscription, cancellationToken);

        var result = false;
        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.UserAccount);
        if (requiresSession)
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

    /// <inheritdoc />
    public async Task<bool> DeadLetterMessageAsync(string topic, string subscription, ServiceBusReceivedMessage message, string source, string reason, string description,
        CancellationToken cancellationToken)
    {
        var requiresSession = await _sessionInfoProvider.RequiresSessionAsync(topic, subscription, cancellationToken);
        var dlqProperties = ServiceBusHelper.GetDeadLetterProperties(source, reason, description);

        var result = false;
        var client = _clientFactory.CreateClient(_context, ServiceBusConnectionType.UserAccount);
        if (requiresSession)
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
