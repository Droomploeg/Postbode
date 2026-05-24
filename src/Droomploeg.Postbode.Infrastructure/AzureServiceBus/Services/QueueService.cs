using Droomploeg.Postbode.Application.ServiceBus.Adapters;
using Droomploeg.Postbode.Application.ServiceBus.Factories;
using Droomploeg.Postbode.Application.ServiceBus.Services;
using Droomploeg.Postbode.Application.Workers.Dispatcher;
using Droomploeg.Postbode.Domain.ServiceBus.Models;
using Droomploeg.Postbode.Domain.Workers.Models;
using Droomploeg.Postbode.Infrastructure.Audit;
using Droomploeg.Postbode.Infrastructure.Contexts;

namespace Droomploeg.Postbode.Infrastructure.AzureServiceBus.Services;

/// <summary>
/// User queue service class.
/// </summary>
/// <typeparam name="TSendMessage">Type of sent message</typeparam>
/// <typeparam name="TReceiveMessage">Type of receiver message</typeparam>
public class QueueService<TSendMessage, TReceiveMessage> : IQueueService<TSendMessage, TReceiveMessage> where TReceiveMessage : class
    where TSendMessage : class
{
    private readonly IContextSetter _contextSetter;
    private readonly IAdapterFactory<IActiveQueueAdapter<TSendMessage, TReceiveMessage>> _activeQueueAdapterFactory;
    private readonly IAdapterFactory<IDeadLetterQueueAdapter<TSendMessage, TReceiveMessage>> _deadLetterAdapterFactory;
    private readonly IWorkerDispatcher _dispatcher;
    private readonly IAuditLogger _auditLogger;

    /// <summary>
    /// Constructor of the queue service.
    /// </summary>
    /// <param name="contextSetter"><see cref="IContextSetter"/></param>"
    /// <param name="activeQueueAdapterFactory"><see cref="IAdapterFactory{T}"/> of <see cref="IActiveQueueAdapter{TSendMessage, TReceiveMessage}"/></param>
    /// <param name="deadLetterAdapterFactory"><see cref="IAdapterFactory{T}"/> of <see cref="IDeadLetterQueueAdapter{TSendMessage, TReceiveMessage}"/></param>
    /// <param name="workerDispatcher"><see cref="IWorkerDispatcher"/></param>
    /// <param name="auditLogger"><see cref="IAuditLogger"/></param>
    public QueueService(
            IContextSetter contextSetter,
            IAdapterFactory<IActiveQueueAdapter<TSendMessage, TReceiveMessage>> activeQueueAdapterFactory,
            IAdapterFactory<IDeadLetterQueueAdapter<TSendMessage, TReceiveMessage>> deadLetterAdapterFactory,
            IWorkerDispatcher workerDispatcher,
            IAuditLogger auditLogger)
    {
        _contextSetter = contextSetter ?? throw new ArgumentNullException(nameof(contextSetter));
        _activeQueueAdapterFactory = activeQueueAdapterFactory ?? throw new ArgumentNullException(nameof(activeQueueAdapterFactory));
        _deadLetterAdapterFactory = deadLetterAdapterFactory ?? throw new ArgumentNullException(nameof(deadLetterAdapterFactory));
        _dispatcher = workerDispatcher ?? throw new ArgumentNullException(nameof(workerDispatcher));
        _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
    }

    /// <inheritdoc cref="IQueueService{TSendMessage, TReceiveMessage}.SendMessageAsync(string, TSendMessage, CancellationToken)"/>
    public async Task<bool> SendMessageAsync(string queue, TSendMessage message, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        await _auditLogger.LogAuditAsync(nameof(SendMessageAsync), queue, $"MessageType: {typeof(TSendMessage).Name}");
        
        var adapter = _activeQueueAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.SendAsync(queue, message, cancellationToken);
    }

    /// <inheritdoc cref="IQueueService{TSendMessage, TReceiveMessage}.PeekActiveMessagesAsync(string, long, int, CancellationToken)"/>
    public async Task<ICollection<TReceiveMessage>> PeekActiveMessagesAsync(
        string queue,
        long fromSequenceNumber,
        int numberOfMessages,
        CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        await _auditLogger.LogAuditAsync(nameof(PeekActiveMessagesAsync), queue, $"FromSeq: {fromSequenceNumber}, Count: {numberOfMessages}");
        
        var adapter = _activeQueueAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.PeekMessagesAsync(queue, fromSequenceNumber, numberOfMessages, cancellationToken);
    }

    /// <inheritdoc cref="IQueueService{TSendMessage, TReceiveMessage}.DeleteActiveMessageAsync(string, TReceiveMessage, CancellationToken)"/>
    public async Task<bool> DeleteActiveMessageAsync(string queue, TReceiveMessage message, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        await _auditLogger.LogAuditAsync(nameof(DeleteActiveMessageAsync), queue);
        
        var adapter = _activeQueueAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.DeleteMessageAsync(queue, message, cancellationToken);
    }

    /// <inheritdoc cref="IQueueService{TSendMessage, TReceiveMessage}.DeleteAllActiveMessagesAsync(string, CancellationToken)"/>
    public async Task<bool> DeleteAllActiveMessagesAsync(string queue, CancellationToken cancellationToken = default)
    {
        var context = await _contextSetter.GetAndUpdateAsync();
        await _auditLogger.LogAuditAsync(nameof(DeleteActiveMessageAsync), queue, "Bulk operation");
        
        var adapter = _activeQueueAdapterFactory.Create(AdapterMode.ManagedIdentity);
        var workItem = new WorkerItem(
            context.UserName,
            context.CorrelationId,
            queue,
            $"Delete all message from '{queue}'",
            (token) => adapter.DeleteAllMessagesAsync(
                queue, token));

        _dispatcher.Dispatch(workItem);
        return true;
    }

    /// <inheritdoc cref="IQueueService{TSendMessage, TReceiveMessage}.DeadLetterMessageAsync(string, TReceiveMessage, string, string, string, CancellationToken)"/>
    public async Task<bool> DeadLetterMessageAsync(string queue, TReceiveMessage message, string source, string reason, string description, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        await _auditLogger.LogAuditAsync(nameof(DeadLetterMessageAsync), queue, $"Reason: {reason}");
        
        var adapter = _activeQueueAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.DeadLetterMessageAsync(queue, message, source, reason, description, cancellationToken);
    }

    /// <inheritdoc cref="IQueueService{TSendMessage, TReceiveMessage}.PeekDeadLetterMessagesAsync(string, long, int, CancellationToken)"/>
    public async Task<ICollection<TReceiveMessage>> PeekDeadLetterMessagesAsync(
        string queue,
        long fromSequenceNumber,
        int numberOfMessages,
        CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        await _auditLogger.LogAuditAsync(nameof(PeekDeadLetterMessagesAsync), queue, $"FromSeq: {fromSequenceNumber}, Count: {numberOfMessages}");
        
        var adapter = _deadLetterAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.PeekMessagesAsync(queue, fromSequenceNumber, numberOfMessages, cancellationToken);
    }

    /// <inheritdoc cref="IQueueService{TSendMessage, TReceiveMessage}.ResubmitMessageAsync(string, TReceiveMessage, TSendMessage, ResubmitOptions, CancellationToken)"/>
    public async Task<bool> ResubmitMessageAsync(string queue,
        TReceiveMessage receivedMessage, TSendMessage repairedMessage,
        ResubmitOptions options, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        await _auditLogger.LogAuditAsync(nameof(ResubmitMessageAsync), queue, $"Options: {options}");
        
        var adapter = _deadLetterAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.ResubmitMessageAsync(queue, receivedMessage, repairedMessage, options, cancellationToken);
    }

    /// <inheritdoc cref="IQueueService{TSendMessage, TReceiveMessage}.ResubmitAllMessagesAsync(string, ResubmitOptions, CancellationToken)"/>
    public async Task<bool> ResubmitAllMessagesAsync(string queue, ResubmitOptions options, CancellationToken cancellationToken = default)
    {
        var context = await _contextSetter.GetAndUpdateAsync();
        await _auditLogger.LogAuditAsync(nameof(ResubmitAllMessagesAsync), queue, $"Bulk operation, Options: {options}");
        
        var adapter = _deadLetterAdapterFactory.Create(AdapterMode.ManagedIdentity);
        var workItem = new WorkerItem(
            context.UserName,
            context.CorrelationId,
            queue,
            $"Resubmit all dead letter messages from '{queue}'",
            (token) => adapter.ResubmitAllMessagesAsync(
                queue, options, token));

        _dispatcher.Dispatch(workItem);
        return true;
    }

    /// <inheritdoc cref="IQueueService{TSendMessage, TReceiveMessage}.DeleteDeadLetterMessageAsync(string, TReceiveMessage, CancellationToken)"/>
    public async Task<bool> DeleteDeadLetterMessageAsync(string queue, TReceiveMessage message, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        await _auditLogger.LogAuditAsync(nameof(DeleteDeadLetterMessageAsync), queue);
        
        var adapter = _deadLetterAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.DeleteMessageAsync(queue, message, cancellationToken);
    }

    /// <inheritdoc cref="IQueueService{TSendMessage, TReceiveMessage}.DeleteAllDeadLetterMessagesAsync(string, CancellationToken)"/>
    public async Task<bool> DeleteAllDeadLetterMessagesAsync(string queue, CancellationToken cancellationToken = default)
    {
        var context = await _contextSetter.GetAndUpdateAsync();
        await _auditLogger.LogAuditAsync(nameof(DeleteAllDeadLetterMessagesAsync), queue, "Bulk operation");
        
        var adapter = _deadLetterAdapterFactory.Create(AdapterMode.ManagedIdentity);
        var workItem = new WorkerItem(
            context.UserName,
            context.CorrelationId,
            queue,
            description: $"Delete all dead letter message from '{queue}'",
            (token) => adapter.DeleteAllMessagesAsync(
                queue, token));

        _dispatcher.Dispatch(workItem);
        return true;
    }
}
