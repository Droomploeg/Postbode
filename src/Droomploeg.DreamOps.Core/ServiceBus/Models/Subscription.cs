using Droomploeg.DreamOps.Domain.ServiceBus.Types;

namespace Droomploeg.DreamOps.Domain.ServiceBus.Models;

/// <summary>
/// Subscription entity.
/// </summary>
/// <param name="Name">Name of the subscription</param>
/// <param name="TopicName">Name of the <see cref="Topic"/></param>
/// <param name="RuntimeState"><see cref="RuntimeState"/></param>
/// <param name="HealthState"><see cref="HealthState"/></param>
/// <param name="EnableBatchedOperations"><see langword="true"/> enable batch operations</param>
/// <param name="EnableDeadLetteringOnFilterEvaluationExceptions"><see langword="true"/> enable dead letter on filter evaluation exceptions</param>
/// <param name="RequiresSession"><see langword="true"/> require session</param>
/// <param name="MaxDeliveryCount">Max delivery count</param>
/// <param name="ForwardTo">Name of the entity to forward to</param>
/// <param name="ForwardDeadLetteredMessagesTo">Name of the entity to forward dead letter messages to</param>
/// <param name="LockDuration"><see cref="TimeSpan"/> lock duration</param>
/// <param name="AutoDeleteOnIdle"><see cref="TimeSpan"/> auto delete on idle</param>
/// <param name="DefaultMessageTimeToLive"><see cref="TimeSpan"/> default message time to live</param>
/// <param name="RuntimeInfo"><see cref="RuntimeInfo"/></param>
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

