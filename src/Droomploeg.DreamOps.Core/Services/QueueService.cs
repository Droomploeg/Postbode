using Droomploeg.DreamOps.Core.Models;
using Droomploeg.DreamOps.Core.Repositories;

namespace Droomploeg.DreamOps.Core.Services;

public class QueueService<TSendMessage, TReceiveMessage>(IQueueRepository<TSendMessage, TReceiveMessage> repository)
    where TReceiveMessage : class
    where TSendMessage : class
{
    private readonly IQueueRepository<TSendMessage, TReceiveMessage> _repository = repository;

    #region [Active Queue]
    public async Task SendMessageAsync(string queue, ICollection<TSendMessage> message, CancellationToken cancellationToken = default)
        => await _repository.SendAsync(queue, message, cancellationToken);

    public async Task<IEnumerable<TReceiveMessage>> PeekAsync(
        string queue,
        long fromSequenceNumber,
        int numberOfMessages,
        CancellationToken cancellationToken = default)
        => await _repository.PeekActiveMessagesAsync(queue, fromSequenceNumber, numberOfMessages, cancellationToken);

    public async Task DeleteAllActiveMessagesAsync(string queue, CancellationToken cancellationToken = default)
        => await _repository.DeleteAllActiveMessagesAsync(queue, cancellationToken);

    public async Task<bool> DeleteActiveMessageAsync(string queue, TReceiveMessage message, CancellationToken cancellationToken = default)
        => await _repository.DeleteActiveMessageAsync(queue, message, cancellationToken);

    public async Task<bool> DeadLetterMessageAsync(string queue, TReceiveMessage message, string source, string reason, string description, CancellationToken cancellationToken = default)
    => await _repository.DeadLetterActiveMessagesAsync(queue, message, source, reason, description, cancellationToken);


    #endregion [Active Queue]

    #region [Dead-Letter Queue]

    public async Task<IEnumerable<TReceiveMessage>> PeekDeadletterAsync(
        string queue,
        long fromSequenceNumber,
        int numberOfMessages,
        CancellationToken cancellationToken = default)
        => await _repository.PeekDeadLetterMessagesAsync(queue, fromSequenceNumber, numberOfMessages, cancellationToken);

    public async Task ResubmitAllDeadletterMessagesAsync(string queue, ResubmitOptions options, CancellationToken cancellationToken = default)
        => await _repository.ResubmitAllDeadLetterMessagesAsync(queue, options, cancellationToken);

    public async Task DeleteAllDeadLetterMessagesAsync(string queue, CancellationToken cancellationToken = default)
        => await _repository.DeleteAllDeadLetterMessagesAsync(queue, cancellationToken);

    public async Task<bool> ResubmitDeadletterMessageAsync(string queue,
        TReceiveMessage receivedMessage, TSendMessage repairedMessage,
        ResubmitOptions options, CancellationToken cancellationToken = default)
    => await _repository.ResubmitDeadLetterMessageAsync(
        queue, receivedMessage, repairedMessage, options, cancellationToken);

    public async Task<bool> DeleteDeadletterMessageAsync(string queue, TReceiveMessage message, CancellationToken cancellationToken = default)
        => await _repository.DeleteDeadLetterMessageAsync(queue, message, cancellationToken);

    #endregion [Dead-Letter Queue]
}
