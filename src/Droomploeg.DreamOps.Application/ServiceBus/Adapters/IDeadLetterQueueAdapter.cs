using Droomploeg.DreamOps.Domain.ServiceBus.Models;

namespace Droomploeg.DreamOps.Application.ServiceBus.Adapters;

/// <summary>
/// Deadletter queue repository interface.
/// </summary>
/// <typeparam name="TSendMessage">Outgoing servicebus message</typeparam>
/// <typeparam name="TReceiveMessage">Incoming servicebus message</typeparam>
public interface IDeadLetterQueueAdapter<TSendMessage, TReceiveMessage>
    where TReceiveMessage : notnull
{
    /// <summary>
    /// Peek dead-letter messages from the queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="fromSequenceNumber">From sequence number</param>
    /// <param name="numberOfMessages">Number of messages</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="ICollection{T}"/> with <see cref="TReceiveMessage"/></returns>
    Task<ICollection<TReceiveMessage>> PeekMessagesAsync(string queue,
        long fromSequenceNumber = EntityAdapterConstants.DefaultStartIndex,
        int numberOfMessages = EntityAdapterConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resubmit all messages from the dead-letter queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="options"><see cref="ResubmitOptions"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Task"></returns>
    Task ResubmitAllMessagesAsync(string queue,
        ResubmitOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete all dead-letter messages from the queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Task"></returns>
    Task DeleteAllMessagesAsync(string queue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resubmit single dead-letter message from the queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="receivedMessage"><see cref="TReceiveMessage"></param>
    /// <param name="sendMessage"><typeparamref name="TSendMessage"/></param>
    /// <param name="options"><see cref="ResubmitOptions"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Task"></returns>
    Task<bool> ResubmitMessageAsync(string queue,
        TReceiveMessage receivedMessage,
        TSendMessage sendMessage,
        ResubmitOptions options,
        CancellationToken cancellationToken);

    /// <summary>
    /// Delete dead-letter message from the queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="message"><see cref="TReceiveMessage"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>Ttrue if deleted</returns>
    Task<bool> DeleteMessageAsync(string queue,
        TReceiveMessage message,
        CancellationToken cancellationToken = default);
}
