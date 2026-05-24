using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.Postbode.Domain.ServiceBus.Models;

namespace Droomploeg.Postbode.Infrastructure.AzureServiceBus.Mappers;

/// <summary>Maps Azure Service Bus queue properties to domain <see cref="Queue"/> model.</summary>
[ExcludeFromCodeCoverage( Justification = "Mapper class")]
internal static class QueueMapper
{
    /// <summary>Maps a <see cref="QueueProperties"/> and <see cref="QueueRuntimeProperties"/> to a <see cref="Queue"/>.</summary>
    /// <param name="queueProperties">The queue configuration properties.</param>
    /// <param name="runtimeProperties">The queue runtime properties containing message counts.</param>
    /// <returns>A domain <see cref="Queue"/> instance.</returns>
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

