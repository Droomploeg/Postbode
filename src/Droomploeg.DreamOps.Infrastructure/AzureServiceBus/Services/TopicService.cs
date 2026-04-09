using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Application.ServiceBus.Factories;
using Droomploeg.DreamOps.Application.ServiceBus.Services;
using Droomploeg.DreamOps.Application.Workers.Dispatcher;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Droomploeg.DreamOps.Domain.Workers.Models;
using Droomploeg.DreamOps.Infrastructure.Audit;
using Droomploeg.DreamOps.Infrastructure.Contexts;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Services;

/// <summary>
/// User topic service class.
/// </summary>
/// <typeparam name="TSendMessage">Type of sent message</typeparam>
/// <typeparam name="TReceiveMessage">Type of received message</typeparam>
public class TopicService<TSendMessage, TReceiveMessage> : ITopicService<TSendMessage, TReceiveMessage> where TSendMessage : class
    where TReceiveMessage : class
{
    private readonly IContextSetter _contextSetter;
    private readonly IAdapterFactory<IActiveTopicAdapter<TSendMessage, TReceiveMessage>> _activeTopicAdapterFactory;
    private readonly IAdapterFactory<IDeadLetterTopicAdapter<TSendMessage, TReceiveMessage>> _deadLetterAdapterFactory;
    private readonly IWorkerDispatcher _dispatcher;
    private readonly IAuditLogger _auditLogger;

    /// <summary>
    /// Constructor of the topic service.
    /// </summary>
    /// <param name="contextSetter"><see cref="IContextSetter"/></param>"
    /// <param name="activeTopicAdapterFactory"><see cref="IAdapterFactory{T}"/> of <see cref="IActiveTopicAdapter{TSendMessage, TReceiveMessage}"/></param>
    /// <param name="deadLetterAdapterFactory"><see cref="IAdapterFactory{T}"/> of <see cref="IDeadLetterTopicAdapter{TSendMessage, TReceiveMessage}"/></param>
    /// <param name="dispatcher"></param>
    /// <param name="auditLogger"><see cref="IAuditLogger"/></param>
    public TopicService(
            IContextSetter contextSetter,
            IAdapterFactory<IActiveTopicAdapter<TSendMessage, TReceiveMessage>> activeTopicAdapterFactory,
            IAdapterFactory<IDeadLetterTopicAdapter<TSendMessage, TReceiveMessage>> deadLetterAdapterFactory,
            IWorkerDispatcher dispatcher,
            IAuditLogger auditLogger)
    {
        _contextSetter = contextSetter ?? throw new ArgumentNullException(nameof(contextSetter));
        _activeTopicAdapterFactory = activeTopicAdapterFactory ?? throw new ArgumentNullException(nameof(activeTopicAdapterFactory));
        _deadLetterAdapterFactory = deadLetterAdapterFactory ?? throw new ArgumentNullException(nameof(deadLetterAdapterFactory));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
    }

    /// <inheritdoc cref="ITopicService{TSendMessage, TReceiveMessage}.SendMessageAsync(string, TSendMessage, CancellationToken)"/>
    public async Task<bool> SendMessageAsync(string topic, TSendMessage message, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        await _auditLogger.LogAuditAsync(nameof(SendMessageAsync), topic, $"MessageType: {typeof(TSendMessage).Name}");
        
        var adapter = _activeTopicAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.SendAsync(topic, message, cancellationToken);
    }

    /// <inheritdoc cref="ITopicService{TSendMessage, TReceiveMessage}.PeekActiveMessagesAsync(string, string, long, int, CancellationToken)"/>
    public async Task<ICollection<TReceiveMessage>> PeekActiveMessagesAsync(
        string topic,
        string subscription,
        long fromSequenceNumber,
        int numberOfMessages,
        CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        await _auditLogger.LogAuditAsync(nameof(PeekActiveMessagesAsync), $"{topic}/{subscription}", $"FromSeq: {fromSequenceNumber}, Count: {numberOfMessages}");
       
        var adapter = _activeTopicAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.PeekMessagesAsync(topic, subscription, fromSequenceNumber, numberOfMessages, cancellationToken);
    }
    
    /// <inheritdoc cref="ITopicService{TSendMessage, TReceiveMessage}.DeleteAllActiveMessagesAsync(string, string, CancellationToken)"/>
    public async Task<bool> DeleteAllActiveMessagesAsync(string topic, string subscription, CancellationToken cancellationToken = default)
    {
        var context = await _contextSetter.GetAndUpdateAsync();
        await _auditLogger.LogAuditAsync(nameof(DeleteAllActiveMessagesAsync), $"{topic}/{subscription}", "Bulk operation");
        
        var adapter = _activeTopicAdapterFactory.Create(AdapterMode.ManagedIdentity);
        var workItem = new WorkerItem(
            context.UserName,
            context.CorrelationId,
            $"{topic}\\{subscription}",
            $"Delete all message from '{topic}/{subscription}'",
            (token) => adapter.DeleteAllMessagesAsync(topic, subscription, token));

        _dispatcher.Dispatch(workItem);
        return true;
    }
    
    /// <inheritdoc cref="ITopicService{TSendMessage, TReceiveMessage}.DeleteActiveMessageAsync(string, string, TReceiveMessage, CancellationToken)"/>
    public async Task<bool> DeleteActiveMessageAsync(string topic, string subscription, TReceiveMessage activeMessage, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        await _auditLogger.LogAuditAsync(nameof(DeleteActiveMessageAsync), $"{topic}/{subscription}");
        
        var adapter = _activeTopicAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.DeleteMessageAsync(topic, subscription, activeMessage, cancellationToken);
    }

    /// <inheritdoc cref="ITopicService{TSendMessage, TReceiveMessage}.DeadLetterMessageAsync(string, string, TReceiveMessage, string, string, string, CancellationToken)"/>
    public async Task<bool> DeadLetterMessageAsync(string topic, string subscription, TReceiveMessage deadletterMessage, string source, string reason, string description, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        await _auditLogger.LogAuditAsync(nameof(DeadLetterMessageAsync), $"{topic}/{subscription}", $"Reason: {reason}");
        
        var adapter = _activeTopicAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.DeadLetterMessageAsync(topic, subscription, deadletterMessage, source, reason, description, cancellationToken);
    }

    /// <inheritdoc cref="ITopicService{TSendMessage, TReceiveMessage}.PeekDeadLetterMessagesAsync(string, string, long, int, CancellationToken)" />
    public async Task<ICollection<TReceiveMessage>> PeekDeadLetterMessagesAsync(
        string topic,
        string subscription,
        long fromSequenceNumber,
        int numberOfMessages,
        CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        await _auditLogger.LogAuditAsync(nameof(PeekDeadLetterMessagesAsync), $"{topic}/{subscription}", $"FromSeq: {fromSequenceNumber}, Count: {numberOfMessages}");

        var adapter = _deadLetterAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.PeekMessagesAsync(topic, subscription, fromSequenceNumber, numberOfMessages, cancellationToken);
    }

    /// <inheritdoc cref="ITopicService{TSendMessage, TReceiveMessage}.ResubmitMessageAsync(string, string, TReceiveMessage, TSendMessage, ResubmitOptions, CancellationToken)"/>
    public async Task<bool> ResubmitMessageAsync(string topic, string subscription,
        TReceiveMessage receivedMessage, TSendMessage repairedMessage,
        ResubmitOptions options, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        await _auditLogger.LogAuditAsync(nameof(ResubmitMessageAsync), $"{topic}/{subscription}", $"Options: {options}");
        
        var adapter = _deadLetterAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.ResubmitMessageAsync(
            topic, subscription, receivedMessage, repairedMessage, options, cancellationToken);
    }

    /// <inheritdoc cref="ITopicService{TSendMessage, TReceiveMessage}.ResubmitAllMessagesAsync(string, string, ResubmitOptions, CancellationToken)"/>
    public async Task<bool> ResubmitAllMessagesAsync(string topic, string subscription, ResubmitOptions options, CancellationToken cancellationToken = default)
    {
        var context = await _contextSetter.GetAndUpdateAsync();
        await _auditLogger.LogAuditAsync(nameof(ResubmitAllMessagesAsync), $"{topic}/{subscription}", $"Bulk operation, Options: {options}");
        
        var adapter = _deadLetterAdapterFactory.Create(AdapterMode.ManagedIdentity);
        var workItem = new WorkerItem(
            context.UserName,
            context.CorrelationId,
            $"{topic}\\{subscription}",
            $"Resubmit all message from topic",
            (token) => adapter.ResubmitAllMessagesAsync(topic, subscription, options, token));

        _dispatcher.Dispatch(workItem);
        return true;
    }

    /// <inheritdoc cref="ITopicService{TSendMessage, TReceiveMessage}.DeleteDeadLetterMessageAsync(string, string, TReceiveMessage, CancellationToken)"/>
    public async Task<bool> DeleteDeadLetterMessageAsync(string topic, string subscription, TReceiveMessage message, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        await _auditLogger.LogAuditAsync(nameof(DeleteDeadLetterMessageAsync), $"{topic}/{subscription}");
        
        var adapter = _deadLetterAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.DeleteMessageAsync(
            topic, subscription, message, cancellationToken);
    }

    /// <inheritdoc cref="ITopicService{TSendMessage, TReceiveMessage}.DeleteAllDeadLetterMessagesAsync(string, string, CancellationToken)"/>
    public async Task<bool> DeleteAllDeadLetterMessagesAsync(string topic, string subscription, CancellationToken cancellationToken = default)
    {
        var context = await _contextSetter.GetAndUpdateAsync();
        await _auditLogger.LogAuditAsync(nameof(DeleteAllDeadLetterMessagesAsync), $"{topic}/{subscription}", "Bulk operation");
        
        var adapter = _deadLetterAdapterFactory.Create(AdapterMode.ManagedIdentity);
        var workItem = new WorkerItem(
            context.UserName,
            context.CorrelationId,
            $"{topic}\\{subscription}",
            $"Delete all message from '{topic}/{subscription}'",
            (token) => adapter.DeleteAllMessagesAsync(topic, subscription, token));

        _dispatcher.Dispatch(workItem);
        return true;
    }
}
