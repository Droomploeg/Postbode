using Model = Droomploeg.DreamOps.Domain.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Mappers;

internal static class QueueMapper
{
    internal static Queue Map(QueueProperties queueProperties, QueueRuntimeProperties runtimeProperties)
        => new(
        Name: queueProperties.Name,
        RuntimeState: EntityRuntimeStateMapper.Map(queueProperties.Status),
        HealthState: EntityHealthStateMapper.Map(runtimeProperties.ActiveMessageCount, runtimeProperties.ScheduledMessageCount, runtimeProperties.TransferMessageCount, runtimeProperties.DeadLetterMessageCount),
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

