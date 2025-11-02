using Droomploeg.DreamOps.Domain.ServiceBus.Models;

namespace Droomploeg.DreamOps.Application.ServiceBus.Adapters;

/// <summary>
/// Deadletter topic repository interface.
/// </summary>
/// <typeparam name="TSendMessage">Outgoing servicebus message</typeparam>
/// <typeparam name="TReceiveMessage">Incoming servicebus message</typeparam>
public interface IDeadLetterTopicAdapter<TSendMessage, TReceiveMessage>
{
    /// <summary>
    /// Peek dead-letter messages from the topic.
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
    /// Resubmit all messages from the dead-letter topic.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="options"><see cref="ResubmitOptions"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Task"></returns>
    Task ResubmitAllMessagesAsync(string topic, string subscription,
        ResubmitOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete all dead-letter messages from the topic.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Task"></returns>
    Task DeleteAllMessagesAsync(string topic, string subscription,
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
    Task<bool> ResubmitMessageAsync(string topic, string subscription,
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
    /// <returns><see langword="true"/> if deleted</returns>
    Task<bool> DeleteMessageAsync(string topic, string subscription,
        TReceiveMessage message,
        CancellationToken cancellationToken = default);
}
