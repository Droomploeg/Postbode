using ServiceBus = Azure.Messaging.ServiceBus.Administration;
using Model = Droomploeg.DreamOps.Core.Models;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Mappers;

internal static class RuntimeMapper
{
    internal static Model.EntityRuntimeInfo Map(
        ServiceBus.QueueRuntimeProperties properties)
        => new(
            properties.TotalMessageCount > 0,
            properties.ActiveMessageCount,
            properties.DeadLetterMessageCount,
            properties.ScheduledMessageCount,
            DateTimeOffset.UtcNow);

    internal static Model.EntityRuntimeInfo Map(
        ServiceBus.SubscriptionRuntimeProperties subscriptionProperties,
        ServiceBus.TopicRuntimeProperties topicProperties)
        => new(
            subscriptionProperties.TotalMessageCount > 0,
            subscriptionProperties.ActiveMessageCount,
            subscriptionProperties.DeadLetterMessageCount,
            topicProperties.ScheduledMessageCount,
            DateTimeOffset.UtcNow);
}



