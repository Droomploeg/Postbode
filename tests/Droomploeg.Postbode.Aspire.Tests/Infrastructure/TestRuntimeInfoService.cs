using Droomploeg.Postbode.Application.ServiceBus.Services;
using Droomploeg.Postbode.Domain.ServiceBus.Models;
using Droomploeg.Postbode.Domain.ServiceBus.Types;

namespace Droomploeg.Postbode.Aspire.Tests.Infrastructure;

/// <summary>
/// Test implementation of IRuntimeInfoService that returns pre-configured queue info.
/// The real implementation uses ServiceBusAdministrationClient (HTTP) which the emulator does not support.
/// </summary>
public class TestRuntimeInfoService : IRuntimeInfoService
{
    private readonly HashSet<string> _sessionEntities = [];
    private readonly List<Queue> _queues = [];
    private readonly List<Topic> _topics = [];

    public void RegisterSessionEntity(string name) =>
        _sessionEntities.Add(name);

    public void AddQueue(string name, long activeMessages = 0, long deadLetterMessages = 0, long scheduledMessages = 0) =>
        _queues.Add(CreateTestQueue(name, _sessionEntities.Contains(name),
            CreateRuntimeInfo(activeMessages, deadLetterMessages, scheduledMessages)));

    public void AddTopic(string name, params Subscription[] subscriptions) =>
        _topics.Add(new Topic(
            Name: name,
            RuntimeState: EntityRuntimeState.Active,
            HealthState: EntityHealthState.NoMessages,
            EnableBatchedOperations: true,
            EnablePartitioning: false,
            RequiresDuplicateDetection: false,
            SupportOrdering: false,
            AutoDeleteOnIdle: TimeSpan.MaxValue,
            DefaultMessageTimeToLive: TimeSpan.MaxValue,
            DuplicateDetectionHistoryTimeWindow: TimeSpan.Zero,
            Subscriptions: subscriptions));

    public Subscription CreateSubscription(string topicName, string subscriptionName,
        long activeMessages = 0, long deadLetterMessages = 0, long scheduledMessages = 0) =>
        CreateTestSubscription(topicName, subscriptionName, _sessionEntities.Contains(subscriptionName),
            CreateRuntimeInfo(activeMessages, deadLetterMessages, scheduledMessages));

    private static EntityRuntimeInfo CreateRuntimeInfo(long activeMessages, long deadLetterMessages, long scheduledMessages) => new(
        HasMessages: activeMessages > 0 || deadLetterMessages > 0,
        TransferMessagesCount: 0,
        ActiveMessageCount: activeMessages,
        TransferDeadLetterMessageCount: 0,
        DeadLetterMessageCount: deadLetterMessages,
        ScheduleMessageCount: scheduledMessages,
        TotalMessageCount: activeMessages + deadLetterMessages + scheduledMessages,
        UpdatedAt: DateTimeOffset.UtcNow);

    private static readonly EntityRuntimeInfo EmptyRuntimeInfo = new(
        HasMessages: false,
        TransferMessagesCount: 0,
        ActiveMessageCount: 0,
        TransferDeadLetterMessageCount: 0,
        DeadLetterMessageCount: 0,
        ScheduleMessageCount: 0,
        TotalMessageCount: 0,
        UpdatedAt: DateTimeOffset.UtcNow);

    private static Queue CreateTestQueue(string name, bool requiresSession = false, EntityRuntimeInfo? runtimeInfo = null) => new(
        Name: name,
        RuntimeState: EntityRuntimeState.Active,
        HealthState: EntityHealthState.NoMessages,
        EnableBatchedOperations: true,
        EnablePartitioning: false,
        RequiresDuplicateDetection: false,
        RequiresSession: requiresSession,
        DeadLetteringOnMessageExpiration: false,
        MaxDeliveryCount: 10,
        ForwardTo: string.Empty,
        ForwardDeadLetteredMessagesTo: string.Empty,
        LockDuration: TimeSpan.FromSeconds(30),
        AutoDeleteOnIdle: TimeSpan.MaxValue,
        DefaultMessageTimeToLive: TimeSpan.MaxValue,
        DuplicateDetectionHistoryTimeWindow: TimeSpan.Zero,
        RuntimeInfo: runtimeInfo ?? EmptyRuntimeInfo);

    public Task<ICollection<Queue>> GetAllQueuesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<ICollection<Queue>>(_queues.Count > 0
            ? [.. _queues]
            : [CreateTestQueue("test-send-message")]);

    public Task<Queue?> GetQueueByNameAsync(string name, CancellationToken cancellationToken = default) =>
        Task.FromResult<Queue?>(CreateTestQueue(name, _sessionEntities.Contains(name)));

    public Task<T[]> GetAllAsync<T>() where T : IEntity
    {
        var entities = new List<IEntity>();
        entities.AddRange(_queues.Count > 0 ? _queues : [CreateTestQueue("test-send-message")]);
        entities.AddRange(_topics);
        return Task.FromResult(entities.OfType<T>().ToArray());
    }

    public Task<ICollection<Topic>> GetAllTopicsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<ICollection<Topic>>([.. _topics]);

    public Task<Topic?> GetTopicByNameAsync(string name, CancellationToken cancellationToken = default) =>
        Task.FromResult<Topic?>(null);

    public Task<Subscription?> GetSubscriptionAsync(string topicName, string subscriptionName, CancellationToken cancellationToken = default) =>
        Task.FromResult<Subscription?>(CreateTestSubscription(topicName, subscriptionName, _sessionEntities.Contains(subscriptionName)));

    private static Subscription CreateTestSubscription(string topicName, string subscriptionName, bool requiresSession = false, EntityRuntimeInfo? runtimeInfo = null) => new(
        Name: subscriptionName,
        TopicName: topicName,
        RuntimeState: EntityRuntimeState.Active,
        HealthState: EntityHealthState.NoMessages,
        EnableBatchedOperations: true,
        EnableDeadLetteringOnFilterEvaluationExceptions: false,
        RequiresSession: requiresSession,
        MaxDeliveryCount: 10,
        ForwardTo: string.Empty,
        ForwardDeadLetteredMessagesTo: string.Empty,
        LockDuration: TimeSpan.FromSeconds(30),
        AutoDeleteOnIdle: TimeSpan.MaxValue,
        DefaultMessageTimeToLive: TimeSpan.MaxValue,
        RuntimeInfo: runtimeInfo ?? EmptyRuntimeInfo);
}
