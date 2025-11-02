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

    Task<bool> DeleteAllActiveMessagesAsync(string queue, CancellationToken cancellationToken = default);
    Task<bool> DeleteAllDeadLetterMessagesAsync(string queue, CancellationToken cancellationToken = default);
    Task<bool> DeleteDeadLetterMessageAsync(string queue, TReceiveMessage message, CancellationToken cancellationToken = default);
    Task<ICollection<TReceiveMessage>> PeekActiveMessagesAsync(string queue, long fromSequenceNumber, int numberOfMessages, CancellationToken cancellationToken = default);
    Task<ICollection<TReceiveMessage>> PeekDeadLetterMessagesAsync(string queue, long fromSequenceNumber, int numberOfMessages, CancellationToken cancellationToken = default);
    Task<bool> ResubmitAllMessagesAsync(string queue, ResubmitOptions options, CancellationToken cancellationToken = default);
    Task<bool> ResubmitMessageAsync(string queue, TReceiveMessage receivedMessage, TSendMessage repairedMessage, ResubmitOptions options, CancellationToken cancellationToken = default);
    Task<bool> SendMessageAsync(string queue, TSendMessage message, CancellationToken cancellationToken = default);

    // todo remove: dummy method to force generic type parameters to be used
    Task<bool> LongRunningTaskAsync(string queue, CancellationToken cancellationToken = default);
}
