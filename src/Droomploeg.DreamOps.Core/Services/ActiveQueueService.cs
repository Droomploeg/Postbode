using Droomploeg.DreamOps.Core.Models;
using Droomploeg.DreamOps.Core.Repositories;

namespace Droomploeg.DreamOps.Core.Services;

// service maken voor OBH flow actions
// service maken voor IAM flow actions
// repository moet client name mee krijgen

/// <summary>
/// Active queue service class.
/// </summary>
/// <typeparam name="TSendMessage">Type of send message</typeparam>
/// <typeparam name="TReceiveMessage">Type of receive message</typeparam>
/// <param name="repository"><see cref="IActiveQueueRepository{TSendMessage, TReceiveMessage}"/></param>
public class ActiveQueueService<TSendMessage, TReceiveMessage>(IActiveQueueRepository<TSendMessage, TReceiveMessage> repository)
    where TReceiveMessage : class
    where TSendMessage : class
{
    private readonly IActiveQueueRepository<TSendMessage, TReceiveMessage> _repository = repository;

    public async Task SendMessageAsync(string queue, ICollection<TSendMessage> message, CancellationToken cancellationToken = default)
        => await _repository.SendAsync(queue, message, cancellationToken);

    public async Task<IEnumerable<TReceiveMessage>> PeekAsync(
        string queue,
        long fromSequenceNumber,
        int numberOfMessages,
        CancellationToken cancellationToken = default)
        => await _repository.PeekMessagesAsync(queue, fromSequenceNumber, numberOfMessages, cancellationToken);

    public async Task DeleteAllMessagesAsync(string queue, CancellationToken cancellationToken = default)
        => await _repository.DeleteAllMessagesAsync(queue, cancellationToken);

    public async Task<bool> DeleteMessageAsync(string queue, TReceiveMessage message, CancellationToken cancellationToken = default)
        => await _repository.DeleteMessageAsync(queue, message, cancellationToken);

    public async Task<bool> DeadLetterMessageAsync(string queue, TReceiveMessage message, string source, string reason, string description, CancellationToken cancellationToken = default)
    => await _repository.DeadLetterMessagesAsync(queue, message, source, reason, description, cancellationToken);
}
