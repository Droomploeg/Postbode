using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Mappers;

internal static class RuntimeMapper
{
    internal static EntityRuntimeInfo Map(
        QueueRuntimeProperties properties)
        => new(
            properties.TotalMessageCount > 0,
            properties.TransferMessageCount,
            properties.ActiveMessageCount,
            properties.TransferDeadLetterMessageCount,
            properties.DeadLetterMessageCount,
            properties.ScheduledMessageCount,
            properties.TotalMessageCount,
            DateTimeOffset.UtcNow);

    internal static EntityRuntimeInfo Map(
        SubscriptionRuntimeProperties subscriptionProperties,
        TopicRuntimeProperties topicProperties)
        => new(
            subscriptionProperties.TotalMessageCount > 0,
            subscriptionProperties.TransferMessageCount,
            subscriptionProperties.ActiveMessageCount,
            subscriptionProperties.TransferDeadLetterMessageCount,
            subscriptionProperties.DeadLetterMessageCount,
            topicProperties.ScheduledMessageCount,
            subscriptionProperties.TotalMessageCount,
            DateTimeOffset.UtcNow);
}



