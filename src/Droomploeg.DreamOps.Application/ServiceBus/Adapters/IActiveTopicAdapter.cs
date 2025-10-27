namespace Droomploeg.DreamOps.Application.ServiceBus.Adapters;

/// <summary>
/// Topic repository interface.
/// </summary>
/// <typeparam name="TSendMessage">Outgoing servicebus message</typeparam>
/// <typeparam name="TReceiveMessage">Incoming servicebus message</typeparam>
public interface IActiveTopicAdapter<TSendMessage, TReceiveMessage>
    where TSendMessage : notnull
    where TReceiveMessage : notnull
{
    /// <summary>
    /// Send message.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="message"><see cref="ICollection{T}"/> of <see cref="TSendMessage"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Task"></returns>
    Task SendAsync(string topic,
        ICollection<TSendMessage> message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Peek messages from the topic and subscription.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="fromSequenceNumber">From sequence number</param>
    /// <param name="numberOfMessages">Number of messages</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="ICollection{T}"/> with <see cref="TReceiveMessage"/></returns>
    Task<ICollection<TReceiveMessage>> PeekMessagesAsync(string topic, string subscription,
        long fromSequenceNumber = EntityAdapterConstants.DefaultStartIndex,
        int numberOfMessages = EntityAdapterConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Delete all active messages from the topic.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Task"></returns>
    Task DeleteAllMessagesAsync(string topic, string subscription,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete first active message from the topic.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="message"><see cref="TReceiveMessage"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if deleted</returns>
    Task<bool> DeleteMessageAsync(string topic, string subscription,
        TReceiveMessage message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Dead-letter first active message from the topic.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="message"><see cref="TReceiveMessage"/></param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="source">Dead-letter source</param>
    /// <param name="reason">Reason of deadlettering</param>
    /// <param name="description">Description of deadlettering</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if dead-lettered</returns>
    Task<bool> DeadLetterMessagesAsync(string topic, string subscription,
        TReceiveMessage message,
        string source,
        string reason,
        string description,
        CancellationToken cancellationToken = default);
}
