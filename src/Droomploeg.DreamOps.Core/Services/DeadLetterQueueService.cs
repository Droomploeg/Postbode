using Droomploeg.DreamOps.Core.Models;
using Droomploeg.DreamOps.Core.Repositories;

namespace Droomploeg.DreamOps.Core.Services;

/// <summary>
/// Dead-letter queue service class.
/// </summary>
/// <typeparam name="TSendMessage">Type of send message</typeparam>
/// <typeparam name="TReceiveMessage">Type of receive message</typeparam>
/// <param name="repository"><see cref="IDeadLetterQueueRepository{TSendMessage, TReceiveMessage}"/></param>
public class DeadLetterQueueService<TSendMessage, TReceiveMessage>(IDeadLetterQueueRepository<TSendMessage, TReceiveMessage> repository)
    where TReceiveMessage : class
    where TSendMessage : class
{
    private readonly IDeadLetterQueueRepository<TSendMessage, TReceiveMessage> _repository = repository;

    public async Task<IEnumerable<TReceiveMessage>> PeekAsync(
        string queue,
        long fromSequenceNumber,
        int numberOfMessages,
        CancellationToken cancellationToken = default)
        => await _repository.PeekMessagesAsync(queue, fromSequenceNumber, numberOfMessages, cancellationToken);

    public async Task ResubmitAllMessagesAsync(string queue, ResubmitOptions options, CancellationToken cancellationToken = default)
        => await _repository.ResubmitAllMessagesAsync(queue, options, cancellationToken);

    public async Task DeleteAllMessagesAsync(string queue, CancellationToken cancellationToken = default)
        => await _repository.DeleteAllMessagesAsync(queue, cancellationToken);

    public async Task<bool> ResubmitMessageAsync(string queue,
        TReceiveMessage receivedMessage, TSendMessage repairedMessage,
        ResubmitOptions options, CancellationToken cancellationToken = default)
    => await _repository.ResubmitMessageAsync(
        queue, receivedMessage, repairedMessage, options, cancellationToken);

    public async Task<bool> DeleteMessageAsync(string queue, TReceiveMessage message, CancellationToken cancellationToken = default)
        => await _repository.DeleteMessageAsync(queue, message, cancellationToken);
}
