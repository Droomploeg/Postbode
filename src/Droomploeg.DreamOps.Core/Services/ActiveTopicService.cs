using Droomploeg.DreamOps.Core.Repositories;

namespace Droomploeg.DreamOps.Core.Services;

/// <summary>
/// Active topic service class.
/// </summary>
/// <typeparam name="TSendMessage">Type of send message</typeparam>
/// <typeparam name="TReceiveMessage">Type of receive message</typeparam>
/// <param name="repository"><see cref="IActiveTopicRepository{TSendMessage, TReceiveMessage}"/></param>
public class ActiveTopicService<TSendMessage, TReceiveMessage>(IActiveTopicRepository<TSendMessage, TReceiveMessage> repository)
    where TSendMessage : class
    where TReceiveMessage : class
{
    private readonly IActiveTopicRepository<TSendMessage, TReceiveMessage> _repository = repository;

    public async Task SendMessageAsync(string topic, ICollection<TSendMessage> message, CancellationToken cancellationToken = default)
    => await _repository.SendAsync(topic, message, cancellationToken);

    public async Task<IEnumerable<TReceiveMessage>> PeekAsync(
        string topic,
        string subscription,
        long fromSequenceNumber,
        int numberOfMessages,
        CancellationToken cancellationToken = default)
        => await _repository.PeekMessagesAsync(topic, subscription, fromSequenceNumber, numberOfMessages, cancellationToken);

    public async Task DeleteAllMessagesAsync(string topic, string subscription, CancellationToken cancellationToken = default)
        => await _repository.DeleteAllMessagesAsync(topic, subscription, cancellationToken);

    public async Task<bool> DeleteMessageAsync(string topic, string subscription, TReceiveMessage activeMessage, CancellationToken cancellationToken = default)
        => await _repository.DeleteMessageAsync(topic, subscription, activeMessage, cancellationToken);

    public async Task<bool> DeadLetterMessageAsync(string topic, string subscription, TReceiveMessage deadletterMessage, string source, string reason, string description, CancellationToken cancellationToken = default)
    => await _repository.DeadLetterMessagesAsync(topic, subscription, deadletterMessage, source, reason, description, cancellationToken);

}
