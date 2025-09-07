namespace Droomploeg.DreamOps.Core.Repositories;

/// <summary>
/// Active queue repository interface.
/// </summary>
/// <typeparam name="TSendMessage">Outgoing servicebus message</typeparam>
/// <typeparam name="TReceiveMessage">Incoming servicebus message</typeparam>
public interface IActiveQueueRepository<TSendMessage, TReceiveMessage>
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
    Task<IEnumerable<TReceiveMessage>> PeekMessagesAsync(string queue,
        long fromSequenceNumber = RepositoryConstants.DefaultStartIndex,
        int numberOfMessages = RepositoryConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete all active messages from the queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Task"></returns>
    Task DeleteAllMessagesAsync(string queue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete first active message from the queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="message"><see cref="TReceiveMessage"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if deleted</returns>
    Task<bool> DeleteMessageAsync(
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
    Task<bool> DeadLetterMessagesAsync(string queue,
        TReceiveMessage message,
        string source,
        string reason,
        string description,
        CancellationToken cancellationToken = default);
}
