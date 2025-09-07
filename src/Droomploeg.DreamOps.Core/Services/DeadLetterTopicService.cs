using Droomploeg.DreamOps.Core.Models;
using Droomploeg.DreamOps.Core.Repositories;

namespace Droomploeg.DreamOps.Core.Services;

/// <summary>
/// Dead-letter topic service class.
/// </summary>
/// <typeparam name="TSendMessage">Type of send message</typeparam>
/// <typeparam name="TReceiveMessage">Type of receive message</typeparam>
/// <param name="repository"><see cref="IDeadLetterTopicRepository{TSendMessage, TReceiveMessage}"/></param>
public class DeadLetterTopicService<TSendMessage, TReceiveMessage>(IDeadLetterTopicRepository<TSendMessage, TReceiveMessage> repository)
    where TSendMessage : class
    where TReceiveMessage : class
{
    private readonly IDeadLetterTopicRepository<TSendMessage, TReceiveMessage> _repository = repository;

    public async Task<IEnumerable<TReceiveMessage>> PeekAsync(
        string topic,
        string subscription,
        long fromSequenceNumber,
        int numberOfMessages,
        CancellationToken cancellationToken = default)
        => await _repository.PeekMessagesAsync(topic, subscription, fromSequenceNumber, numberOfMessages, cancellationToken);

    public async Task ResubmitAllMessagesAsync(string topic, string subscription, ResubmitOptions options, CancellationToken cancellationToken = default)
        => await _repository.ResubmitAllMessagesAsync(topic, subscription, options, cancellationToken);

    public async Task DeleteAllMessagesAsync(string topic, string subscription, CancellationToken cancellationToken = default)
        => await _repository.DeleteAllMessagesAsync(topic, subscription, cancellationToken);

    public async Task<bool> ResubmitMessageAsync(string topic, string subscription,
        TReceiveMessage receivedMessage, TSendMessage repairedMessage,
        ResubmitOptions options, CancellationToken cancellationToken = default)
    => await _repository.ResubmitMessageAsync(
        topic, subscription, receivedMessage, repairedMessage, options, cancellationToken);

    public async Task<bool> DeleteMessageAsync(string topic, string subscription, TReceiveMessage message, CancellationToken cancellationToken = default)
        => await _repository.DeleteMessageAsync(topic, subscription, message, cancellationToken);

}

