using Droomploeg.DreamOps.Domain.ServiceBus.Types;

namespace Droomploeg.DreamOps.Domain.ServiceBus.Models;

/// <summary>
/// Topic entity.
/// </summary>
/// <param name="Name">Name of the topic</param>
/// <param name="RuntimeState"><see cref="RuntimeState"/></param>
/// <param name="HealthState"><see cref="HealthState"/></param>
/// <param name="EnableBatchedOperations"><see langword="true"/> when enable batch operations</param>
/// <param name="EnablePartitioning"><see langword="true"/> when enable partioning</param>
/// <param name="RequiresDuplicateDetection"><see langword="true"/> when require duplication dectection</param>
/// <param name="SupportOrdering"><see langword="true"/> when supporting ordering</param>
/// <param name="AutoDeleteOnIdle"><see cref="TimeSpan"/> auto delete on idle</param>
/// <param name="DefaultMessageTimeToLive"><see cref="TimeSpan"/> default message time to live</param>
/// <param name="DuplicateDetectionHistoryTimeWindow"><see cref="TimeSpan"/> duplication dectected history time window</param>
/// <param name="Subscriptions"><see cref=Array"/> of <see cref="Subscription"/></param>
public record Topic(
    string Name,
    EntityRuntimeState RuntimeState,
    EntityHealthState HealthState,
    bool EnableBatchedOperations,
    bool EnablePartitioning,
    bool RequiresDuplicateDetection,
    bool SupportOrdering,
    TimeSpan AutoDeleteOnIdle,
    TimeSpan DefaultMessageTimeToLive,
    TimeSpan DuplicateDetectionHistoryTimeWindow,
    Subscription[] Subscriptions) : IEntity;
