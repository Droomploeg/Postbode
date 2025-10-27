using Droomploeg.DreamOps.Domain.ServiceBus.Types;

namespace Droomploeg.DreamOps.Domain.ServiceBus.Models;

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
