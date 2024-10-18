using Droomploeg.DreamOps.Core.Models;
using Droomploeg.DreamOps.Core.Repositories;

namespace Droomploeg.DreamOps.Core.Services;

public class TopicService<TSendMessage, TReceiveMessage>(ITopicRepository<TSendMessage, TReceiveMessage> repository)
    where TSendMessage : class
    where TReceiveMessage : class
{
    private readonly ITopicRepository<TSendMessage, TReceiveMessage> _repository = repository;

    #region [Active Queue]
    public async Task SendMessageAsync(string topic, ICollection<TSendMessage> message, CancellationToken cancellationToken = default)
    => await _repository.SendAsync(topic, message, cancellationToken);

    public async Task<IEnumerable<TReceiveMessage>> PeekAsync(
        string topic,
        string subscription,
        long fromSequenceNumber,
        int numberOfMessages,
        CancellationToken cancellationToken = default)
        => await _repository.PeekActiveMessagesAsync(topic, subscription, fromSequenceNumber, numberOfMessages, cancellationToken);

    public async Task DeleteAllActiveMessagesAsync(string topic, string subscription, CancellationToken cancellationToken = default)
        => await _repository.DeleteAllActiveMessagesAsync(topic, subscription, cancellationToken);

    public async Task<bool> DeleteActiveMessageAsync(string topic, string subscription, TReceiveMessage activeMessage, CancellationToken cancellationToken = default)
        => await _repository.DeleteActiveMessageAsync(topic, subscription, activeMessage, cancellationToken);

    public async Task<bool> DeadLetterMessageAsync(string topic, string subscription, TReceiveMessage deadletterMessage, string source, string reason, string description, CancellationToken cancellationToken = default)
    => await _repository.DeadLetterMessagesAsync(topic, subscription, deadletterMessage, source, reason, description, cancellationToken);


    #endregion [Active Queue]

    #region [Dead-Letter Queue]

    public async Task<IEnumerable<TReceiveMessage>> PeekDeadletterAsync(
        string topic,
        string subscription,
        long fromSequenceNumber,
        int numberOfMessages,
        CancellationToken cancellationToken = default)
        => await _repository.PeekDeadLetterMessagesAsync(topic, subscription, fromSequenceNumber, numberOfMessages, cancellationToken);

    public async Task ResubmitAllDeadletterMessagesAsync(string topic, string subscription, ResubmitOptions options, CancellationToken cancellationToken = default)
        => await _repository.ResubmitAllDeadLetterMessagesAsync(topic, subscription, options, cancellationToken);

    public async Task DeleteAllDeadLetterMessagesAsync(string topic, string subscription, CancellationToken cancellationToken = default)
        => await _repository.DeleteAllDeadLetterMessagesAsync(topic, subscription, cancellationToken);

    public async Task<bool> ResubmitDeadletterMessageAsync(string topic, string subscription,
        TReceiveMessage receivedMessage, TSendMessage repairedMessage,
        ResubmitOptions options, CancellationToken cancellationToken = default)
    => await _repository.ResubmitDeadLetterMessageAsync(
        topic, subscription, receivedMessage, repairedMessage, options, cancellationToken);


    public async Task<bool> DeleteDeadletterMessageAsync(string topic, string subscription, TReceiveMessage message, CancellationToken cancellationToken = default)
        => await _repository.DeleteDeadLetterMessageAsync(topic, subscription, message, cancellationToken);

    #endregion [Dead-Letter Queue]
}
