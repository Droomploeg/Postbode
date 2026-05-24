using Droomploeg.Postbode.Domain.ServiceBus.Models;
using Droomploeg.Postbode.Domain.ServiceBus.Types;

namespace Droomploeg.Postbode.WebApp.Common;

public static class DefaultEntityFactory
{
    public static Queue DefaultQueue(string name)
        => new (
            Name: name,
            RuntimeState: EntityRuntimeState.Unknown,
            HealthState: EntityHealthState.Unknown,
            EnableBatchedOperations: false,
            EnablePartitioning: false,
            RequiresDuplicateDetection: false,
            RequiresSession: false,
            DeadLetteringOnMessageExpiration: false,
            MaxDeliveryCount: 0,
            ForwardTo: string.Empty,
            ForwardDeadLetteredMessagesTo: string.Empty,
            LockDuration: TimeSpan.Zero,
            AutoDeleteOnIdle: TimeSpan.Zero,
            DefaultMessageTimeToLive: TimeSpan.Zero,
            DuplicateDetectionHistoryTimeWindow: TimeSpan.Zero,
            RuntimeInfo: RuntimeInfo);

    public static Topic DefaultTopic(string name)
        => new (
            Name: name,
            RuntimeState: EntityRuntimeState.Unknown,
            HealthState: EntityHealthState.Unknown,
            EnableBatchedOperations: false,
            EnablePartitioning: false,
            RequiresDuplicateDetection: false,
            SupportOrdering: false,
            AutoDeleteOnIdle: TimeSpan.Zero,
            DefaultMessageTimeToLive: TimeSpan.Zero,
            DuplicateDetectionHistoryTimeWindow: TimeSpan.Zero,
            Subscriptions: []);

    public static Subscription DefaultSubscription(string topic, string subscription)
        => new (
            Name: subscription,
            TopicName: topic,
            RuntimeState: EntityRuntimeState.Unknown,
            HealthState: EntityHealthState.Unknown,
            EnableBatchedOperations: false,
            EnableDeadLetteringOnFilterEvaluationExceptions: false,
            RequiresSession: false,
            MaxDeliveryCount: 0,
            ForwardTo: string.Empty,
            ForwardDeadLetteredMessagesTo: string.Empty,
            LockDuration: TimeSpan.Zero,
            AutoDeleteOnIdle: TimeSpan.Zero,
            DefaultMessageTimeToLive: TimeSpan.Zero,
            RuntimeInfo: RuntimeInfo
            );

    private static EntityRuntimeInfo RuntimeInfo
    {
        get
        { 
            return new(
                HasMessages: false,
                TransferMessagesCount: 0,
                ActiveMessageCount: 0,
                DeadLetterMessageCount: 0,
                TransferDeadLetterMessageCount: 0,
                ScheduleMessageCount: 0,
                TotalMessageCount: 0,
                UpdatedAt: DateTimeOffset.MinValue);
        } 
    }
}
