using ServiceBus = Azure.Messaging.ServiceBus.Administration;
using Model = Droomploeg.DreamOps.Core.Models;
using Droomploeg.DreamOps.Core.Models;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Mappers;

internal static class SubscriptionMapper
{
    internal static Model.Subscription Map(
        ServiceBus.SubscriptionProperties subscriptionProperties,
        ServiceBus.SubscriptionRuntimeProperties subscriptionRuntimeProperties,
        ServiceBus.TopicRuntimeProperties topicRuntimeProperties)
    {
        return new(
                Name: subscriptionProperties.SubscriptionName,
                TopicName: subscriptionProperties.TopicName,
                RuntimeState: EntityRuntimeStateMapper.Map(subscriptionProperties.Status),
                HealthState: EntityHealthStateMapper.Map(0, subscriptionRuntimeProperties.ActiveMessageCount, subscriptionRuntimeProperties.DeadLetterMessageCount),
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
