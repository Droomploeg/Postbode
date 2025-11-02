using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Application.ServiceBus.Factories;
using Droomploeg.DreamOps.Application.ServiceBus.Services;
using Droomploeg.DreamOps.Application.Workers.Dispatcher;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Droomploeg.DreamOps.Domain.Workers.Models;
using Droomploeg.DreamOps.Infrastructure.Contexts;
using Microsoft.Azure.Amqp.Framing;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Services;

/// <summary>
/// User queue service class.
/// </summary>
/// <typeparam name="TSendMessage">Type of send message</typeparam>
/// <typeparam name="TReceiveMessage">Type of receive message</typeparam>
public class QueueService<TSendMessage, TReceiveMessage> : IQueueService<TSendMessage, TReceiveMessage> where TReceiveMessage : class
    where TSendMessage : class
{
    private readonly IContextSetter _contextSetter;
    private readonly IAdapterFactory<IActiveQueueAdapter<TSendMessage, TReceiveMessage>> _activeQueueAdapterFactory;
    private readonly IAdapterFactory<IDeadLetterQueueAdapter<TSendMessage, TReceiveMessage>> _deadletterAdapterFactory;
    private readonly IWorkerDispatcher _dispatcher;

    /// <summary> 
    /// Constructor of the queue service.
    /// </summary>
    /// <param name="contextSetter"><see cref="IContextSetter"/></param>"
    /// <param name="activeQueueAdapterFactory"><see cref="IAdapterFactory{T}"/> of <see cref="IActiveQueueAdapter{TSendMessage, TReceiveMessage}"/></param>
    /// <param name="deadLetterAdapterFactory"><see cref="IAdapterFactory{T}"/> of <see cref="IDeadLetterQueueAdapter{TSendMessage, TReceiveMessage}"/></param>
    /// <param name="dispatcherFactory"><see cref="ICommandDispatcher"/></param>
    public QueueService(
            IContextSetter contextSetter,
            IAdapterFactory<IActiveQueueAdapter<TSendMessage, TReceiveMessage>> activeQueueAdapterFactory,
            IAdapterFactory<IDeadLetterQueueAdapter<TSendMessage, TReceiveMessage>> deadLetterAdapterFactory,
            IWorkerDispatcher workerDispatcher)
    {
        _contextSetter = contextSetter ?? throw new ArgumentNullException(nameof(contextSetter));
        _activeQueueAdapterFactory = activeQueueAdapterFactory ?? throw new ArgumentNullException(nameof(activeQueueAdapterFactory));
        _deadletterAdapterFactory = deadLetterAdapterFactory ?? throw new ArgumentNullException(nameof(deadLetterAdapterFactory));
        _dispatcher = workerDispatcher ?? throw new ArgumentNullException(nameof(workerDispatcher));
    }

    /// <inheritdoc cref="IQueueService{TSendMessage, TReceiveMessage}.SendMessageAsync(string, TSendMessage, CancellationToken)"/>
    public async Task<bool> SendMessageAsync(string queue, TSendMessage message, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        var adapter = _activeQueueAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.SendAsync(queue, [message], cancellationToken);
    }

    /// <inheritdoc cref="IQueueService{TSendMessage, TReceiveMessage}.PeekActiveMessagesAsync(string, long, int, CancellationToken)"/>
    public async Task<ICollection<TReceiveMessage>> PeekActiveMessagesAsync(
        string queue,
        long fromSequenceNumber,
        int numberOfMessages,
        CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        var adapter = _activeQueueAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.PeekMessagesAsync(queue, fromSequenceNumber, numberOfMessages, cancellationToken);
    }

    /// <inheritdoc cref="IQueueService{TSendMessage, TReceiveMessage}.DeleteActiveMessageAsync(string, TReceiveMessage, CancellationToken)"/>
    public async Task<bool> DeleteActiveMessageAsync(string queue, TReceiveMessage message, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        var adapter = _activeQueueAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.DeleteMessageAsync(queue, message, cancellationToken);
    }

    /// <inheritdoc cref="IQueueService{TSendMessage, TReceiveMessage}.DeleteAllActiveMessagesAsync(string, CancellationToken)"/>
    public async Task<bool> DeleteAllActiveMessagesAsync(string queue, CancellationToken cancellationToken = default)
    {
        var context = await _contextSetter.GetAndUpdateAsync();
        var adapter = _activeQueueAdapterFactory.Create(AdapterMode.ManagedIdentity);
        var workItem = new WorkerItem(
            context.UserName,
            queue,
            $"Delete all message from queue",
            (token) => adapter.DeleteAllMessagesAsync(
                queue, token));

        _dispatcher.Dispatch(workItem);
        return true;
    }

    /// <inheritdoc cref="IQueueService{TSendMessage, TReceiveMessage}.DeadLetterMessageAsync(string, TReceiveMessage, string, string, string, CancellationToken)"/>
    public async Task<bool> DeadLetterMessageAsync(string queue, TReceiveMessage message, string source, string reason, string description, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        var adapter = _activeQueueAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.DeadLetterMessagesAsync(queue, message, source, reason, description, cancellationToken);
    }

    /// <inheritdoc cref="IQueueService{TSendMessage, TReceiveMessage}.PeekDeadLetterMessagesAsync(string, long, int, CancellationToken)"/>
    public async Task<ICollection<TReceiveMessage>> PeekDeadLetterMessagesAsync(
        string queue,
        long fromSequenceNumber,
        int numberOfMessages,
        CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        var adapter = _deadletterAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.PeekMessagesAsync(queue, fromSequenceNumber, numberOfMessages, cancellationToken);
    }

    /// <inheritdoc cref="IQueueService{TSendMessage, TReceiveMessage}.ResubmitMessageAsync(string, TReceiveMessage, TSendMessage, ResubmitOptions, CancellationToken)"/>
    public async Task<bool> ResubmitMessageAsync(string queue,
        TReceiveMessage receivedMessage, TSendMessage repairedMessage,
        ResubmitOptions options, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        var adapter = _deadletterAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.ResubmitMessageAsync(queue, receivedMessage, repairedMessage, options, cancellationToken);
    }

    /// <inheritdoc cref="IQueueService{TSendMessage, TReceiveMessage}.ResubmitAllMessagesAsync(string, ResubmitOptions, CancellationToken)"/>
    public async Task<bool> ResubmitAllMessagesAsync(string queue, ResubmitOptions options, CancellationToken cancellationToken = default)
    {
        var context = await _contextSetter.GetAndUpdateAsync();
        var adpater = _deadletterAdapterFactory.Create(AdapterMode.ManagedIdentity);
        var workItem = new WorkerItem(
            context.UserName,
            queue,
            $"Resubmit all deadletter messages from queue",
            (token) => adpater.ResubmitAllMessagesAsync(
                queue, options, token));

        _dispatcher.Dispatch(workItem);
        return true;
    }

    /// <inheritdoc cref="IQueueService{TSendMessage, TReceiveMessage}.DeleteDeadLetterMessageAsync(string, TReceiveMessage, CancellationToken)"/>
    public async Task<bool> DeleteDeadLetterMessageAsync(string queue, TReceiveMessage message, CancellationToken cancellationToken = default)
    {
        _ = await _contextSetter.GetAndUpdateAsync();
        var adapter = _deadletterAdapterFactory.Create(AdapterMode.OnBehalfOf);
        return await adapter.DeleteMessageAsync(queue, message, cancellationToken);
    }

    /// <inheritdoc cref="IQueueService{TSendMessage, TReceiveMessage}.DeleteAllDeadLetterMessagesAsync(string, CancellationToken)"/>
    public async Task<bool> DeleteAllDeadLetterMessagesAsync(string queue, CancellationToken cancellationToken = default)
    {
        var context = await _contextSetter.GetAndUpdateAsync();
        var adapter = _deadletterAdapterFactory.Create(AdapterMode.ManagedIdentity);
        var workItem = new WorkerItem(
            context.UserName,
            queue,
            $"Delete all deadletter message from queue",
            (token) => adapter.DeleteAllMessagesAsync(
                queue, token));

        _dispatcher.Dispatch(workItem);
        return true;
    }

    // todo remove: dummy method to force generic type parameters to be used
    public async Task<bool> LongRunningTaskAsync(string queue, CancellationToken cancellationToken = default)
    {
        var context = await _contextSetter.GetAndUpdateAsync();
        var adapter = _deadletterAdapterFactory.Create(AdapterMode.ManagedIdentity);
        var workItem = new WorkerItem(
            context.UserName,
            queue,
            $"Dummy task for queue",
            (token) => DoNothing(token)
        );

        _dispatcher.Dispatch(workItem);
        return true;

    }

    private async Task DoNothing(CancellationToken cancellationToken)
    {
        while (cancellationToken.IsCancellationRequested == false)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }
}
