using Droomploeg.DreamOps.Core.Models;

namespace Droomploeg.DreamOps.Core.Repositories;

/// <summary>
/// Topic repository interface.
/// </summary>
/// <typeparam name="TSendMessage">Outgoing servicebus message</typeparam>
/// <typeparam name="TReceiveMessage">Incoming servicebus message</typeparam>
public interface ITopicRepository<TSendMessage, TReceiveMessage>
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
    /// <returns><see cref="IEnumerable{T}"/> with <see cref="TReceiveMessage"/></returns>
    Task<IEnumerable<TReceiveMessage>> PeekActiveMessagesAsync(string topic, string subscription,
        long fromSequenceNumber = RepositoryConstants.DefaultStartIndex,
        int numberOfMessages = RepositoryConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Delete all active messages from the topic.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Task"></returns>
    Task DeleteAllActiveMessagesAsync(string topic, string subscription, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete first active message from the topic.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="message"><see cref="TReceiveMessage"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if deleted</returns>
    Task<bool> DeleteActiveMessageAsync(string topic, string subscription, 
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

    /// <summary>
    /// Peek dead-letter messages from the topic.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="fromSequenceNumber">From sequence number</param>
    /// <param name="numberOfMessages">Number of messages</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="IEnumerable{T}"/> with <see cref="TReceiveMessage"/></returns>
    Task<IEnumerable<TReceiveMessage>> PeekDeadLetterMessagesAsync(string topic, string subscription,
        long fromSequenceNumber = RepositoryConstants.DefaultStartIndex,
        int numberOfMessages = RepositoryConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resubmit all messages from the dead-letter topic.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="options"><see cref="ResubmitOptions"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Task"></returns>
    Task ResubmitAllDeadLetterMessagesAsync(string topic, string subscription, 
        ResubmitOptions options, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete all dead-letter messages from the topic.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Task"></returns>
    Task DeleteAllDeadLetterMessagesAsync(string topic, string subscription, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resubmit single dead-letter message from the topic.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="receivedMessage"><see cref="TReceiveMessage"></param>
    /// <param name="sendMessage"><typeparamref name="TSendMessage"/></param>
    /// <param name="options"><see cref="ResubmitOptions"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Task"></returns>
    Task<bool> ResubmitDeadLetterMessageAsync(string topic, string subscription,
        TReceiveMessage receivedMessage, 
        TSendMessage sendMessage,
        ResubmitOptions options, 
        CancellationToken cancellationToken);

    /// <summary>
    /// Delete dead-letter message from the topic.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="message"><see cref="TReceiveMessage"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>Ttrue if deleted</returns>
    Task<bool> DeleteDeadLetterMessageAsync(string topic, string subscription, 
        TReceiveMessage message, 
        CancellationToken cancellationToken = default);
}
