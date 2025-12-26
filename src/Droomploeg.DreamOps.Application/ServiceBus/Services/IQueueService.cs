using Droomploeg.DreamOps.Domain.ServiceBus.Models;

namespace Droomploeg.DreamOps.Application.ServiceBus.Services;

/// <summary>
/// Interface for queue service.
/// </summary>
/// <typeparam name="TSendMessage">Send message type</typeparam>
/// <typeparam name="TReceiveMessage">Receive message type</typeparam>
public interface IQueueService<TSendMessage, TReceiveMessage>
    where TSendMessage : class
    where TReceiveMessage : class
{
    /// <summary>
    /// Deal-letter first active message from the queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="message"><see cref="TReceiveMessage"></param>
    /// <param name="source">Name of the source</param>
    /// <param name="reason">Reason for dead-letter the message</param>
    /// <param name="description">More detail about the dead-letter reason</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if succeed</returns>
    Task<bool> DeadLetterMessageAsync(string queue, TReceiveMessage message, string source, string reason, string description, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete first active message from the queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="message"><see cref="TReceiveMessage"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if deleted</returns>
    Task<bool> DeleteActiveMessageAsync(string queue, TReceiveMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete all active messages from the queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if all active message are delete</returns>
    Task<bool> DeleteAllActiveMessagesAsync(string queue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete all dead-letter messages from the queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if all dead-letter messages are deleted</returns>
    Task<bool> DeleteAllDeadLetterMessagesAsync(string queue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete dead-letter message from the queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="message"><see cref="TReceiveMessage"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if deleted</returns>
    Task<bool> DeleteDeadLetterMessageAsync(string queue, TReceiveMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Peek active messages from the queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="fromSequenceNumber">Sequence number to start peeking from</param>
    /// <param name="numberOfMessages">Number of messages to peek</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>Collection of peeked messages</returns>
    Task<ICollection<TReceiveMessage>> PeekActiveMessagesAsync(string queue, long fromSequenceNumber, int numberOfMessages, CancellationToken cancellationToken = default);

    /// <summary>
    /// Peek dead-letter messages from the queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="fromSequenceNumber">Sequence number to start peeking from</param>
    /// <param name="numberOfMessages">Number of messages to peek</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>Collection of peeked messages</returns>
    Task<ICollection<TReceiveMessage>> PeekDeadLetterMessagesAsync(string queue, long fromSequenceNumber, int numberOfMessages, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resubmit all messages from the dead-letter queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="options">Resubmit options</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if all messages were resubmitted</returns>
    Task<bool> ResubmitAllMessagesAsync(string queue, ResubmitOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resubmit a message from the dead-letter queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="receivedMessage"><see cref="TReceiveMessage"/></param>
    /// <param name="repairedMessage"><see cref="TSendMessage"/></param>
    /// <param name="options">Resubmit options</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if the message was resubmitted</returns>  
    Task<bool> ResubmitMessageAsync(string queue, TReceiveMessage receivedMessage, TSendMessage repairedMessage, ResubmitOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send message to the queue.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="message"><see cref="TSendMessage"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>True if the message was sent</returns>
    Task<bool> SendMessageAsync(string queue, TSendMessage message, CancellationToken cancellationToken = default);

    // todo remove: dummy method to force generic type parameters to be used
    Task<bool> LongRunningTaskAsync(string queue, CancellationToken cancellationToken = default);
}
