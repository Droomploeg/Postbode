using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Mappers;

/// <summary>Maps Azure Service Bus topic properties to domain <see cref="Topic"/> model.</summary>
[ExcludeFromCodeCoverage( Justification = "Mapper class")]
internal static class TopicMapper
{
    /// <summary>Maps a <see cref="TopicProperties"/> with its subscriptions to a <see cref="Topic"/>.</summary>
    /// <param name="azureTopic">The topic configuration properties.</param>
    /// <param name="azureTopicRuntime">The topic runtime properties containing message counts.</param>
    /// <param name="azureSubscriptions">The subscription configuration properties belonging to this topic.</param>
    /// <param name="azureSubscriptionRuntimes">The subscription runtime properties belonging to this topic.</param>
    /// <returns>A domain <see cref="Topic"/> instance with mapped subscriptions.</returns>
    internal static Topic Map(
        TopicProperties azureTopic,
        TopicRuntimeProperties azureTopicRuntime,
        List<SubscriptionProperties> azureSubscriptions,
        List<SubscriptionRuntimeProperties> azureSubscriptionRuntimes)
    {
        return new Topic(
                Name: azureTopic.Name,
                RuntimeState: EntityRuntimeStateMapper.Map(azureTopic.Status),
                HealthState: EntityHealthStateMapper.Map(azureSubscriptionRuntimes.Sum(s => s.ActiveMessageCount), azureTopicRuntime.ScheduledMessageCount, azureSubscriptionRuntimes.Sum(s => s.TransferMessageCount), azureSubscriptionRuntimes.Sum(s => s.DeadLetterMessageCount)),
                EnableBatchedOperations: azureTopic.EnableBatchedOperations,
                EnablePartitioning: azureTopic.EnablePartitioning,
                RequiresDuplicateDetection: azureTopic.RequiresDuplicateDetection,
                SupportOrdering: azureTopic.SupportOrdering,
                AutoDeleteOnIdle: azureTopic.AutoDeleteOnIdle,
                DefaultMessageTimeToLive: azureTopic.DefaultMessageTimeToLive,
                DuplicateDetectionHistoryTimeWindow: azureTopic.DuplicateDetectionHistoryTimeWindow,
                Subscriptions: [.. azureSubscriptions.Select(subscription =>
                    SubscriptionMapper.Map(
                        subscription,
                        azureSubscriptionRuntimes.Single(runtime =>
                            runtime.TopicName == azureTopic.Name &&
                            runtime.SubscriptionName == subscription.SubscriptionName),
                        azureTopicRuntime)
                    )]
            );
    }
}
