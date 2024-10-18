namespace Droomploeg.DreamOps.Core.Models;

public record Queue(string Name,
    EntityRuntimeState RuntimeState,
    EntityHealthState HealthState,
    bool EnableBatchedOperations,
    bool EnablePartitioning,
    bool RequiresDuplicateDetection,
    bool RequiresSession,
    bool DeadLetteringOnMessageExpiration,
    int MaxDeliveryCount,
    string ForwardTo,
    string ForwardDeadLetteredMessagesTo,
    TimeSpan LockDuration,
    TimeSpan AutoDeleteOnIdle,
    TimeSpan DefaultMessageTimeToLive,
    TimeSpan DuplicateDetectionHistoryTimeWindow,
    EntityRuntimeInfo RuntimeInfo) : IEntity;
