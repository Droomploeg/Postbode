using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Mappers;

/// <summary>Maps Azure Service Bus subscription properties to domain <see cref="Subscription"/> model.</summary>
[ExcludeFromCodeCoverage( Justification = "Mapper class")]
internal static class SubscriptionMapper
{
    /// <summary>Maps a <see cref="SubscriptionProperties"/> and <see cref="SubscriptionRuntimeProperties"/> to a <see cref="Subscription"/>.</summary>
    /// <param name="subscriptionProperties">The subscription configuration properties.</param>
    /// <param name="subscriptionRuntimeProperties">The subscription runtime properties containing message counts.</param>
    /// <param name="topicRuntimeProperties">The parent topic runtime properties for a scheduled message counts.</param>
    /// <returns>A domain <see cref="Subscription"/> instance.</returns>
    internal static Subscription Map(
        SubscriptionProperties subscriptionProperties,
        SubscriptionRuntimeProperties subscriptionRuntimeProperties,
        TopicRuntimeProperties topicRuntimeProperties)
    {
        return new Subscription(
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
