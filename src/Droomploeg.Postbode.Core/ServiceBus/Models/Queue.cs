using Droomploeg.Postbode.Domain.ServiceBus.Types;

namespace Droomploeg.Postbode.Domain.ServiceBus.Models;

/// <summary>
/// Queue entity.
/// </summary>
/// <param name="Name">Name of the queue</param>
/// <param name="RuntimeState"><see cref="RuntimeState"/></param>
/// <param name="HealthState"><see cref="HealthState"/></param>
/// <param name="EnableBatchedOperations"><see langword="true"/> when enable batch operations</param>
/// <param name="EnablePartitioning"><see langword="true"/> when enable partioning</param>
/// <param name="RequiresDuplicateDetection"><see langword="true"/> when require duplication dectection</param>
/// <param name="RequiresSession"><see langword="true"/> when require session</param>
/// <param name="DeadLetteringOnMessageExpiration"><see langword="true"/> when dead letter on expiration</param>
/// <param name="MaxDeliveryCount">Max delivery count</param>
/// <param name="ForwardTo">Name of the entity to forward to</param>
/// <param name="ForwardDeadLetteredMessagesTo">Name of the entity to forward dead letter messages to</param>
/// <param name="LockDuration"><see cref="TimeSpan"/> lock duration</param>
/// <param name="AutoDeleteOnIdle"><see cref="TimeSpan"/> auto delete on idle</param>
/// <param name="DefaultMessageTimeToLive"><see cref="TimeSpan"/> default message time to live</param>
/// <param name="DuplicateDetectionHistoryTimeWindow"><see cref="TimeSpan"/> duplication dectected history time window</param>
/// <param name="RuntimeInfo"><see cref="RuntimeInfo"/></param>
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
