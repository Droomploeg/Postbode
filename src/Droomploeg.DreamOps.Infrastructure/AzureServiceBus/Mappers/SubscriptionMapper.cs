using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Model = Droomploeg.DreamOps.Domain.ServiceBus;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Mappers;

internal static class SubscriptionMapper
{
    internal static Subscription Map(
        SubscriptionProperties subscriptionProperties,
        SubscriptionRuntimeProperties subscriptionRuntimeProperties,
        TopicRuntimeProperties topicRuntimeProperties)
    {
        return new(
                Name: subscriptionProperties.SubscriptionName,
                TopicName: subscriptionProperties.TopicName,
                RuntimeState: EntityRuntimeStateMapper.Map(subscriptionProperties.Status),
                HealthState: EntityHealthStateMapper.Map(subscriptionRuntimeProperties.ActiveMessageCount, 0, subscriptionRuntimeProperties.TransferMessageCount, subscriptionRuntimeProperties.DeadLetterMessageCount),
                EnableBatchedOperations: subscriptionProperties.EnableBatchedOperations,
                EnableDeadLetteringOnFilterEvaluationExceptions: subscriptionProperties.EnableDeadLetteringOnFilterEvaluationExceptions,
                RequiresSession: subscriptionProperties.RequiresSession,
                MaxDeliveryCount: subscriptionProperties.MaxDeliveryCount,
                ForwardTo: subscriptionProperties.ForwardTo,
                ForwardDeadLetteredMessagesTo: subscriptionProperties.ForwardDeadLetteredMessagesTo,
                LockDuration: subscriptionProperties.LockDuration,
                AutoDeleteOnIdle: subscriptionProperties.AutoDeleteOnIdle,
                DefaultMessageTimeToLive: subscriptionProperties.DefaultMessageTimeToLive,
                RuntimeInfo: RuntimeMapper.Map(subscriptionRuntimeProperties, topicRuntimeProperties)
            );
    }
}
