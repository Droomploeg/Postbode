using Model = Droomploeg.DreamOps.Domain.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Mappers;

internal static class TopicMapper
{
    internal static Topic Map(
        TopicProperties azureTopic,
        TopicRuntimeProperties azureTopicRuntime,
        List<SubscriptionProperties> azureSubscriptions,
        List<SubscriptionRuntimeProperties> azureSubscriptionRuntimes)
    {
        return new(
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
