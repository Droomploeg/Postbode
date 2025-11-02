using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Application.ServiceBus.Factories;
using Droomploeg.DreamOps.Application.ServiceBus.Services;
using Droomploeg.DreamOps.Application.Workers.Dispatcher;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Droomploeg.DreamOps.Domain.Workers.Models;
using Droomploeg.DreamOps.Infrastructure.Contexts;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Services;

/// <summary>
/// User topic service class.
/// </summary>
/// <typeparam name="TSendMessage">Type of send message</typeparam>
/// <typeparam name="TReceiveMessage">Type of receive message</typeparam>
public class TopicService<TSendMessage, TReceiveMessage> : ITopicService<TSendMessage, TReceiveMessage> where TSendMessage : class
    where TReceiveMessage : class
{
    private readonly IContextSetter _contextSetter;
    private readonly IAdapterFactory<IActiveTopicAdapter<TSendMessage, TReceiveMessage>> _activeTopicAdapterFactory;
    private readonly IAdapterFactory<IDeadLetterTopicAdapter<TSendMessage, TReceiveMessage>> _deadLetterAdapterFactory;
    private readonly IWorkerDispatcher _dispatcher;

    /// <summary>
    /// Constructor of the topic service.
    /// </summary>
    /// <param name="contextSetter"><see cref="IContextSetter"/></param>"
    /// <param name="activeTopicAdapterFactory"><see cref="IAdapterFactory{T}"/> of <see cref="IActiveTopicAdapter{TSendMessage, TReceiveMessage}{TSendMessage, TReceiveMessage}"/></param>
    /// <param name="deadLetterAdapterFactory"><see cref="IAdapterFactory{T}"/> of <see cref="IDeadLetterTopicAdapter{TSendMessage, TReceiveMessage}"/></param>
    /// <param name="dispatcher"></param>
    public TopicService(
            IContextSetter contextSetter,
            IAdapterFactory<IActiveTopicAdapter<TSendMessage, TReceiveMessage>> activeTopicAdapterFactory,
            IAdapterFactory<IDeadLetterTopicAdapter<TSendMessage, TReceiveMessage>> deadLetterAdapterFactory,
            IWorkerDispatcher dispatcher)
    {
        _contextSetter = contextSetter ?? throw new ArgumentNullException(nameof(contextSetter));
        _activeTopicAdapterFactory = activeTopicAdapterFactory ?? throw new ArgumentNullException(nameof(activeTopicAdapterFactory));
        _deadLetterAdapterFactory = deadLetterAdapterFactory ?? throw new ArgumentNullException(nameof(deadLetterAdapterFactory));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public async Task<bool> SendMessageAsync(string topic, ICollection<TSendMessage> message, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        var adapter = _activeTopicAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.SendAsync(topic, message, cancellationToken);
    }


    public async Task<ICollection<TReceiveMessage>> PeekActiveMessagesAsync(
        string topic,
        string subscription,
        long fromSequenceNumber,
        int numberOfMessages,
        CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        var adapter = _activeTopicAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.PeekMessagesAsync(topic, subscription, fromSequenceNumber, numberOfMessages, cancellationToken);
    }

    public async Task<bool> DeleteAllActiveMessagesAsync(string topic, string subscription, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        var adapter = _activeTopicAdapterFactory.Create(AdapterMode.ManagedIdentity);

        var workItem = new WorkerItem(
            $"{topic}\\{subscription}",
            $"Delete all message from topic '{topic}' and '{subscription}'",
            (token) => adapter.DeleteAllMessagesAsync(topic, subscription, cancellationToken));

        _dispatcher.Dispatch(workItem);
        return true;
    }

    public async Task<bool> DeleteActiveMessageAsync(string topic, string subscription, TReceiveMessage activeMessage, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        var adapter = _activeTopicAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.DeleteMessageAsync(topic, subscription, activeMessage, cancellationToken);
    }

    public async Task<bool> DeadLetterMessageAsync(string topic, string subscription, TReceiveMessage deadletterMessage, string source, string reason, string description, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        var adapter = _activeTopicAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.DeadLetterMessagesAsync(topic, subscription, deadletterMessage, source, reason, description, cancellationToken);
    }

    public async Task<ICollection<TReceiveMessage>> PeekDeadLetterMessagesAsync(
        string topic,
        string subscription,
        long fromSequenceNumber,
        int numberOfMessages,
        CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        var adapter = _activeTopicAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.PeekMessagesAsync(topic, subscription, fromSequenceNumber, numberOfMessages, cancellationToken);
    }

    public async Task<bool> ResubmitMessageAsync(string topic, string subscription,
        TReceiveMessage receivedMessage, TSendMessage repairedMessage,
        ResubmitOptions options, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        var adapter = _deadLetterAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.ResubmitMessageAsync(
            topic, subscription, receivedMessage, repairedMessage, options, cancellationToken);
    }

    public async Task<bool> ResubmitAllMessagesAsync(string topic, string subscription, ResubmitOptions options, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        var adapter = _deadLetterAdapterFactory.Create(AdapterMode.ManagedIdentity);

        var workItem = new WorkerItem(
            $"{topic}\\{subscription}",
            $"Resubmit all message from topic '{topic}' and '{subscription}'",
            (token) => adapter.ResubmitAllMessagesAsync(topic, subscription, options, cancellationToken));

        _dispatcher.Dispatch(workItem);
        return true;
    }

    public async Task<bool> DeleteDeadLetterMessageAsync(string topic, string subscription, TReceiveMessage message, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        var adapter = _deadLetterAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.DeleteMessageAsync(
            topic, subscription, message, cancellationToken);
    }

    public async Task<bool> DeleteAllDeadLetterMessagesAsync(string topic, string subscription, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        var adapter = _deadLetterAdapterFactory.Create(AdapterMode.ManagedIdentity);

        var workItem = new WorkerItem(
            $"{topic}\\{subscription}",
            $"Delete all message from topic '{topic}' and '{subscription}'",
            (token) => adapter.DeleteAllMessagesAsync(topic, subscription, cancellationToken));

        _dispatcher.Dispatch(workItem);
        return true;
    }
}
