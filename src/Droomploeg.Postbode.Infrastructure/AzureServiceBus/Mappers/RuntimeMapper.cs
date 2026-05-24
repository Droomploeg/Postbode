using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.Postbode.Domain.ServiceBus.Models;

namespace Droomploeg.Postbode.Infrastructure.AzureServiceBus.Mappers;

/// <summary>Maps Azure Service Bus runtime properties to domain <see cref="EntityRuntimeInfo"/> model.</summary>
[ExcludeFromCodeCoverage( Justification = "Mapper class")]
internal static class RuntimeMapper
{
    /// <summary>Maps a <see cref="QueueRuntimeProperties"/> to an <see cref="EntityRuntimeInfo"/>.</summary>
    /// <param name="properties">The queue runtime properties.</param>
    /// <returns>An <see cref="EntityRuntimeInfo"/> with the current message counts.</returns>
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

    /// <summary>Maps a <see cref="SubscriptionRuntimeProperties"/> and <see cref="TopicRuntimeProperties"/> to an <see cref="EntityRuntimeInfo"/>.</summary>
    /// <param name="subscriptionProperties">The subscription runtime properties.</param>
    /// <param name="topicProperties">The parent topic runtime properties for a scheduled message counts.</param>
    /// <returns>An <see cref="EntityRuntimeInfo"/> with the current message counts.</returns>
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



