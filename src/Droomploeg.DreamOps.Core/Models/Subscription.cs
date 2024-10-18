namespace Droomploeg.DreamOps.Core.Models;

public record Subscription(
    string Name,
    string TopicName,
    EntityRuntimeState RuntimeState,
    EntityHealthState HealthState,
    bool EnableBatchedOperations,
    bool EnableDeadLetteringOnFilterEvaluationExceptions,
    bool RequiresSession,
    int MaxDeliveryCount,
    string ForwardTo,
    string ForwardDeadLetteredMessagesTo,
    TimeSpan LockDuration,
    TimeSpan AutoDeleteOnIdle,
    TimeSpan DefaultMessageTimeToLive,
    EntityRuntimeInfo RuntimeInfo) : IEntity;

