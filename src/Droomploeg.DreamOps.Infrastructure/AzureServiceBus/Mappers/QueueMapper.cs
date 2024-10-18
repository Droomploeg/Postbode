using ServiceBus = Azure.Messaging.ServiceBus.Administration;
using Model = Droomploeg.DreamOps.Core.Models;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Mappers;

internal static class QueueMapper
{
    internal static Model.Queue Map(ServiceBus.QueueProperties queueProperties, ServiceBus.QueueRuntimeProperties runtimeProperties)
        => new(
        Name: queueProperties.Name,
        RuntimeState: EntityRuntimeStateMapper.Map(queueProperties.Status),
        HealthState: EntityHealthStateMapper.Map(runtimeProperties.ScheduledMessageCount, runtimeProperties.ActiveMessageCount, runtimeProperties.DeadLetterMessageCount),
        EnableBatchedOperations: queueProperties.EnableBatchedOperations,
        EnablePartitioning: queueProperties.EnablePartitioning,
        RequiresDuplicateDetection: queueProperties.RequiresDuplicateDetection,
        RequiresSession: queueProperties.RequiresSession,
        DeadLetteringOnMessageExpiration: queueProperties.DeadLetteringOnMessageExpiration,
        MaxDeliveryCount: queueProperties.MaxDeliveryCount,
        ForwardTo: queueProperties.ForwardTo,
        ForwardDeadLetteredMessagesTo: queueProperties.ForwardDeadLetteredMessagesTo,
        LockDuration: queueProperties.LockDuration,
        AutoDeleteOnIdle: queueProperties.AutoDeleteOnIdle,
        DefaultMessageTimeToLive: queueProperties.DefaultMessageTimeToLive,
        DuplicateDetectionHistoryTimeWindow: queueProperties.DuplicateDetectionHistoryTimeWindow,
        RuntimeInfo: RuntimeMapper.Map(runtimeProperties));
}

