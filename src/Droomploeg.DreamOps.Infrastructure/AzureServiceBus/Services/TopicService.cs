using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Application.ServiceBus.Services;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Services;

/// <summary>
/// User topic service class.
/// </summary>
/// <typeparam name="TSendMessage">Type of send message</typeparam>
/// <typeparam name="TReceiveMessage">Type of receive message</typeparam>
/// <param name="activeTopicAdapter"><see cref="IActiveTopicAdapter{TSendMessage, TReceiveMessage}"/></param>
/// <param name="deadLetterAdapter"><see cref="IDeadLetterTopicAdapter{TSendMessage, TReceiveMessage}"/></param>
public class TopicService<TSendMessage, TReceiveMessage>(
        IActiveTopicAdapter<TSendMessage, TReceiveMessage> activeTopicAdapter,
        IDeadLetterTopicAdapter<TSendMessage, TReceiveMessage> deadLetterAdapter) : ITopicService<TSendMessage, TReceiveMessage> where TSendMessage : class
    where TReceiveMessage : class
{
    private readonly IActiveTopicAdapter<TSendMessage, TReceiveMessage> _activeTopicAdapter = activeTopicAdapter;
    private readonly IDeadLetterTopicAdapter<TSendMessage, TReceiveMessage> _deadLetterAdapter = deadLetterAdapter;

    public async Task SendMessageAsync(string topic, ICollection<TSendMessage> message, CancellationToken cancellationToken = default)
    => await _activeTopicAdapter.SendAsync(topic, message, cancellationToken);

    public async Task<ICollection<TReceiveMessage>> PeekActiveMessagesAsync(
        string topic,
        string subscription,
        long fromSequenceNumber,
        int numberOfMessages,
        CancellationToken cancellationToken = default)
        => await _activeTopicAdapter.PeekMessagesAsync(topic, subscription, fromSequenceNumber, numberOfMessages, cancellationToken);

    public async Task DeleteAllActiveMessagesAsync(string topic, string subscription, CancellationToken cancellationToken = default)
        => await _activeTopicAdapter.DeleteAllMessagesAsync(topic, subscription, cancellationToken);

    public async Task<bool> DeleteActiveMessageAsync(string topic, string subscription, TReceiveMessage activeMessage, CancellationToken cancellationToken = default)
        => await _activeTopicAdapter.DeleteMessageAsync(topic, subscription, activeMessage, cancellationToken);

    public async Task<bool> DeadLetterMessageAsync(string topic, string subscription, TReceiveMessage deadletterMessage, string source, string reason, string description, CancellationToken cancellationToken = default)
        => await _activeTopicAdapter.DeadLetterMessagesAsync(topic, subscription, deadletterMessage, source, reason, description, cancellationToken);

    public async Task<ICollection<TReceiveMessage>> PeekDeadLetterMessagesAsync(
        string topic,
        string subscription,
        long fromSequenceNumber,
        int numberOfMessages,
        CancellationToken cancellationToken = default)
        => await _deadLetterAdapter.PeekMessagesAsync(topic, subscription, fromSequenceNumber, numberOfMessages, cancellationToken);

    public async Task<bool> ResubmitMessageAsync(string topic, string subscription,
        TReceiveMessage receivedMessage, TSendMessage repairedMessage,
        ResubmitOptions options, CancellationToken cancellationToken = default)
        => await _deadLetterAdapter.ResubmitMessageAsync(
            topic, subscription, receivedMessage, repairedMessage, options, cancellationToken);

    public async Task ResubmitAllMessagesAsync(string topic, string subscription, ResubmitOptions options, CancellationToken cancellationToken = default)
        => await _deadLetterAdapter.ResubmitAllMessagesAsync(topic, subscription, options, cancellationToken);

    public async Task<bool> DeleteDeadLetterMessageAsync(string topic, string subscription, TReceiveMessage message, CancellationToken cancellationToken = default)
        => await _deadLetterAdapter.DeleteMessageAsync(topic, subscription, message, cancellationToken);

    public async Task DeleteAllDeadLetterMessagesAsync(string topic, string subscription, CancellationToken cancellationToken = default)
        => await _deadLetterAdapter.DeleteAllMessagesAsync(topic, subscription, cancellationToken);
}
