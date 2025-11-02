using Droomploeg.DreamOps.Domain.ServiceBus.Models;

namespace Droomploeg.DreamOps.Application.ServiceBus.Services
{
    public interface ITopicService<TSendMessage, TReceiveMessage>
        where TSendMessage : class
        where TReceiveMessage : class
    {
        Task<bool> DeadLetterMessageAsync(string topic, string subscription, TReceiveMessage deadletterMessage, string source, string reason, string description, CancellationToken cancellationToken = default);
        Task<bool> DeleteActiveMessageAsync(string topic, string subscription, TReceiveMessage activeMessage, CancellationToken cancellationToken = default);
        Task<bool> DeleteAllActiveMessagesAsync(string topic, string subscription, CancellationToken cancellationToken = default);
        Task<bool> DeleteAllDeadLetterMessagesAsync(string topic, string subscription, CancellationToken cancellationToken = default);
        Task<bool> DeleteDeadLetterMessageAsync(string topic, string subscription, TReceiveMessage message, CancellationToken cancellationToken = default);
        Task<ICollection<TReceiveMessage>> PeekActiveMessagesAsync(string topic, string subscription, long fromSequenceNumber, int numberOfMessages, CancellationToken cancellationToken = default);
        Task<ICollection<TReceiveMessage>> PeekDeadLetterMessagesAsync(string topic, string subscription, long fromSequenceNumber, int numberOfMessages, CancellationToken cancellationToken = default);
        Task<bool> ResubmitAllMessagesAsync(string topic, string subscription, ResubmitOptions options, CancellationToken cancellationToken = default);
        Task<bool> ResubmitMessageAsync(string topic, string subscription, TReceiveMessage receivedMessage, TSendMessage repairedMessage, ResubmitOptions options, CancellationToken cancellationToken = default);
        Task<bool> SendMessageAsync(string topic, ICollection<TSendMessage> message, CancellationToken cancellationToken = default);
    }
}
