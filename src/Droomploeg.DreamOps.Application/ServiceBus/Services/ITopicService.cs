using Droomploeg.DreamOps.Domain.ServiceBus.Models;

namespace Droomploeg.DreamOps.Application.ServiceBus.Services;

/// <summary>
/// Topic service interface.
/// </summary>
/// <typeparam name="TSendMessage">Send message type</typeparam>
/// <typeparam name="TReceiveMessage">Receive message type</typeparam>
public interface ITopicService<TSendMessage, TReceiveMessage>
    where TSendMessage : class
    where TReceiveMessage : class
{
    /// <summary>
    /// Dead-letter first active message from the topic subscription.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="deadletterMessage"><see cref="TReceiveMessage"/></param>
    /// <param name="source">Name of the source</param>
    /// <param name="reason">Reason for dead-letter the message</param>
    /// <param name="description">More detail about the dead-letter reason</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if succeed</returns>
    Task<bool> DeadLetterMessageAsync(string topic, string subscription, 
        TReceiveMessage deadletterMessage, 
        string source, string reason, string description, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete first active message from the topic subscription.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="activeMessage"><see cref="TReceiveMessage"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if deleted</returns>
    Task<bool> DeleteActiveMessageAsync(string topic, string subscription, 
        TReceiveMessage activeMessage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete all active messages from the topic subscription.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if deleted</returns>
    Task<bool> DeleteAllActiveMessagesAsync(string topic, string subscription, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete all dead-letter messages from the topic subscription.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if deleted</returns>
    Task<bool> DeleteAllDeadLetterMessagesAsync(string topic, string subscription, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete dead-letter message from the topic subscription.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="message"><see cref="TReceiveMessage"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if deleted</returns>
    Task<bool> DeleteDeadLetterMessageAsync(string topic, string subscription, 
        TReceiveMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Peek active messages from the topic subscription.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="fromSequenceNumber">Starting sequence number</param>
    /// <param name="numberOfMessages">Number of messages to peek</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>Collection of active messages</returns>
    Task<ICollection<TReceiveMessage>> PeekActiveMessagesAsync(string topic, string subscription, 
        long fromSequenceNumber, int numberOfMessages, CancellationToken cancellationToken = default);

    /// <summary>
    /// Peek dead-letter messages from the topic subscription.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="fromSequenceNumber">Starting sequence number</param>
    /// <param name="numberOfMessages">Number of messages to peek</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>Collection of dead-letter messages</returns>
    Task<ICollection<TReceiveMessage>> PeekDeadLetterMessagesAsync(string topic, string subscription, 
        long fromSequenceNumber, int numberOfMessages, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resubmit all messages from the dead-letter topic subscription.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="options">Resubmit options</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if resubmitted</returns>
    Task<bool> ResubmitAllMessagesAsync(string topic, string subscription, 
        ResubmitOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resubmit single dead-letter message from the topic subscription.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="receivedMessage"><see cref="TReceiveMessage"/></param>
    /// <param name="repairedMessage"><see cref="TSendMessage"/></param>
    /// <param name="options">Resubmit options</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if resubmitted</returns>
    Task<bool> ResubmitMessageAsync(string topic, string subscription, 
        TReceiveMessage receivedMessage, TSendMessage repairedMessage, ResubmitOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send message to topic.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="message"><see cref="TSendMessage"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if sent</returns>
    Task<bool> SendMessageAsync(string topic, 
        TSendMessage message, CancellationToken cancellationToken = default);
}
