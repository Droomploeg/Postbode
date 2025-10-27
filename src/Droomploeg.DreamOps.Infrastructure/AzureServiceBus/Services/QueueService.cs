using Droomploeg.DreamOps.Application.Common;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Application.ServiceBus.Commands.Queues;
using Droomploeg.DreamOps.Application.ServiceBus.Services;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Dispatchers;
using Droomploeg.DreamOps.Infrastructure.Contexts;
using Microsoft.Extensions.Logging;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Services;

/// <summary>
/// User queue service class.
/// </summary>
/// <typeparam name="TSendMessage">Type of send message</typeparam>
/// <typeparam name="TReceiveMessage">Type of receive message</typeparam>
public class QueueService<TSendMessage, TReceiveMessage> : IQueueService<TSendMessage, TReceiveMessage> where TReceiveMessage : class
    where TSendMessage : class
{
    private readonly WebContextSetter _contextSetter;
    private readonly IActiveQueueAdapter<TSendMessage, TReceiveMessage> _activeQueueAdapter;
    private readonly IDeadLetterQueueAdapter<TSendMessage, TReceiveMessage> _deadletterAdapter;
    private readonly ICommandDispatcherFactory _dispatcherFactory;
    private readonly ILogger<QueueService<TSendMessage, TReceiveMessage>> _logger;

    /// <summary> 
    /// Constructor of the queue service.
    /// </summary>
    /// <param name="contextSetter"><see cref="WebContextSetter"/></param>"
    /// <param name="activeQueueAdapter"><see cref="IActiveQueueAdapter{TSendMessage, TReceiveMessage}"/></param>
    /// <param name="deadLetterAdapter"><see cref="IDeadLetterQueueAdapter{TSendMessage, TReceiveMessage}"/></param>
    /// <param name="dispatcherFactory"><see cref="ICommandDispatcher"/></param>
    /// <param name="logger"><see cref="ILogger{TCategoryName}"/></param>
    public QueueService(
            WebContextSetter contextSetter,
            IActiveQueueAdapter<TSendMessage, TReceiveMessage> activeQueueAdapter,
            IDeadLetterQueueAdapter<TSendMessage, TReceiveMessage> deadLetterAdapter,
            ICommandDispatcherFactory dispatcherFactory,
            ILogger<QueueService<TSendMessage, TReceiveMessage>> logger)
    {
        _contextSetter = contextSetter ?? throw new ArgumentNullException(nameof(contextSetter));
        _activeQueueAdapter = activeQueueAdapter ?? throw new ArgumentNullException(nameof(activeQueueAdapter));
        _deadletterAdapter = deadLetterAdapter ?? throw new ArgumentNullException(nameof(deadLetterAdapter));
        _dispatcherFactory = dispatcherFactory ?? throw new ArgumentNullException(nameof(dispatcherFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> SendMessageAsync(string queue, TSendMessage message, CancellationToken cancellationToken = default)
    {
        var context = await _contextSetter.GetAndUpdateAsync();

        var command = new SendMessageCommand<TSendMessage>(queue, message);
        var dispatcher = _dispatcherFactory.GetDispatcher(context.CurrentConnection);
        return await dispatcher.SendAsync(command, cancellationToken);
    }

    public async Task<ICollection<TReceiveMessage>> PeekActiveMessagesAsync(
        string queue,
        long fromSequenceNumber,
        int numberOfMessages,
        CancellationToken cancellationToken = default)
    {
        await _contextSetter.GetAndUpdateAsync();

        return await _activeQueueAdapter.PeekMessagesAsync(queue, fromSequenceNumber, numberOfMessages, cancellationToken);
    }

    public async Task<bool> DeleteActiveMessageAsync(string queue, TReceiveMessage message, CancellationToken cancellationToken = default)
    {
        var context = await _contextSetter.GetAndUpdateAsync();

        var command = new DeleteActiveMessageCommand<TReceiveMessage>(queue, message);
        var dispatcher = _dispatcherFactory.GetDispatcher(context.CurrentConnection);
        return await dispatcher.SendAsync(command, cancellationToken);
    }

    public async Task<bool> DeleteAllActiveMessagesAsync(string queue, CancellationToken cancellationToken = default)
    {
        var context = await _contextSetter.GetAndUpdateAsync();

        var command = new DeleteAllActiveMessagesCommand(queue);
        var dispatcher = _dispatcherFactory.GetDispatcher(context.CurrentConnection);
        return await dispatcher.SendAsync(command, cancellationToken);
    }

    public async Task<bool> DeadLetterMessageAsync(string queue, TReceiveMessage message, string source, string reason, string description, CancellationToken cancellationToken = default)
    {
        var context = await _contextSetter.GetAndUpdateAsync();

        var command = new DeadLetterMessageCommand<TReceiveMessage>(queue, message, source, reason, description);
        var dispatcher = _dispatcherFactory.GetDispatcher(context.CurrentConnection);
        return await dispatcher.SendAsync(command, cancellationToken);
    }

    public async Task<ICollection<TReceiveMessage>> PeekDeadLetterMessagesAsync(
        string queue,
        long fromSequenceNumber,
        int numberOfMessages,
        CancellationToken cancellationToken = default)
    {
        await _contextSetter.GetAndUpdateAsync();

        return await _deadletterAdapter.PeekMessagesAsync(queue, fromSequenceNumber, numberOfMessages, cancellationToken);
    }

    public async Task<bool> ResubmitMessageAsync(string queue,
        TReceiveMessage receivedMessage, TSendMessage repairedMessage,
        ResubmitOptions options, CancellationToken cancellationToken = default)
    {
        var context = await _contextSetter.GetAndUpdateAsync();

        var command = new ResubmitMessageCommand<TSendMessage, TReceiveMessage>(queue, receivedMessage, repairedMessage, options);
        var dispatcher = _dispatcherFactory.GetDispatcher(context.CurrentConnection);
        return await dispatcher.SendAsync(command, cancellationToken);
    }

    public async Task<bool> ResubmitAllMessagesAsync(string queue, ResubmitOptions options, CancellationToken cancellationToken = default)
    {
        var context = await _contextSetter.GetAndUpdateAsync();

        var command = new ResubmitAllMessagesCommand(queue, options);
        var dispatcher = _dispatcherFactory.GetDispatcher(context.CurrentConnection);
        return await dispatcher.SendAsync(command, cancellationToken);
    }

    public async Task<bool> DeleteDeadLetterMessageAsync(string queue, TReceiveMessage message, CancellationToken cancellationToken = default)
    {
        var context = await _contextSetter.GetAndUpdateAsync();

        var command = new DeleteDeadLetterMessageCommand<TReceiveMessage>(queue, message);
        var dispatcher = _dispatcherFactory.GetDispatcher(context.CurrentConnection);
        return await dispatcher.SendAsync(command, cancellationToken);
    }

    public async Task<bool> DeleteAllDeadLetterMessagesAsync(string queue, CancellationToken cancellationToken = default)
    {
        var context = await _contextSetter.GetAndUpdateAsync();

        var command = new DeleteAllDeadLetterMessagesCommand(queue);
        var dispatcher = _dispatcherFactory.GetDispatcher(context.CurrentConnection);
        return await dispatcher.SendAsync(command, cancellationToken);
    }
}
