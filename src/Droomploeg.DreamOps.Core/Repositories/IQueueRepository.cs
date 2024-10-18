using Droomploeg.DreamOps.Core.Models;

namespace Droomploeg.DreamOps.Core.Repositories;

/// <summary>
/// Queue repository interface.
/// </summary>
/// <typeparam name="TSendMessage">Outgoing servicebus message</typeparam>
/// <typeparam name="TReceiveMessage">Incoming servicebus message</typeparam>
public interface IQueueRepository<TSendMessage, TReceiveMessage>
    where TReceiveMessage : notnull
{
    /// <summary>
    /// Send message.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="message"><see cref="ICollection{T}"/> of <see cref="TSendMessage"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Task"></returns>
    Task SendAsync(string queue, ICollection<TSendMessage> message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Peek messages from the queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="fromSequenceNumber">From sequence number</param>
    /// <param name="numberOfMessages">Number of messages</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="IEnumerable{T}"/> with <see cref="TReceiveMessage"/></returns>
    Task<IEnumerable<TReceiveMessage>> PeekActiveMessagesAsync(string queue,
        long fromSequenceNumber = RepositoryConstants.DefaultStartIndex,
        int numberOfMessages = RepositoryConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Delete all active messages from the queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Task"></returns>
    Task DeleteAllActiveMessagesAsync(string queue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete first active message from the queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="message"><see cref="TReceiveMessage"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if deleted</returns>
    Task<bool> DeleteActiveMessageAsync(
        string queue, 
        TReceiveMessage message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Dead-letter first active message from the queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="message"><see cref="TReceiveMessage"/></param> 
    /// <param name="source">Dead-letter source</param>
    /// <param name="reason">Reason of deadlettering</param>
    /// <param name="description">Description of deadlettering</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if dead-lettered</returns>
    Task<bool> DeadLetterActiveMessagesAsync(string queue,
        TReceiveMessage message,
        string source,
        string reason,
        string description,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Peek dead-letter messages from the queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="fromSequenceNumber">From sequence number</param>
    /// <param name="numberOfMessages">Number of messages</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="IEnumerable{T}"/> with <see cref="TReceiveMessage"/></returns>
    Task<IEnumerable<TReceiveMessage>> PeekDeadLetterMessagesAsync(string queue,
        long fromSequenceNumber = RepositoryConstants.DefaultStartIndex,
        int numberOfMessages = RepositoryConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resubmit all messages from the dead-letter queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="options"><see cref="ResubmitOptions"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Task"></returns>
    Task ResubmitAllDeadLetterMessagesAsync(string queue, 
        ResubmitOptions options, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete all dead-letter messages from the queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Task"></returns>
    Task DeleteAllDeadLetterMessagesAsync(string queue, 
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
    Task<bool> ResubmitDeadLetterMessageAsync(string queue,
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
    Task<bool> DeleteDeadLetterMessageAsync(string queue, 
        TReceiveMessage message, 
        CancellationToken cancellationToken = default);
}
