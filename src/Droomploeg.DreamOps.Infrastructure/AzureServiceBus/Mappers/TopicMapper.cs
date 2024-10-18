using Model = Droomploeg.DreamOps.Core.Models;
using ServiceBus = Azure.Messaging.ServiceBus.Administration;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Mappers;

internal static class TopicMapper
{
    internal static Model.Topic Map(
        ServiceBus.TopicProperties azureTopic,
        ServiceBus.TopicRuntimeProperties azureTopicRuntime,
        List<ServiceBus.SubscriptionProperties> azureSubscriptions,
        List<ServiceBus.SubscriptionRuntimeProperties> azureSubscriptionRuntimes)
    {
        return new(
                Name: azureTopic.Name,
                RuntimeState: EntityRuntimeStateMapper.Map(azureTopic.Status),
                HealthState: EntityHealthStateMapper.Map(azureTopicRuntime.ScheduledMessageCount, azureSubscriptionRuntimes.Sum(s => s.ActiveMessageCount), azureSubscriptionRuntimes.Sum(s => s.DeadLetterMessageCount)),
                EnableBatchedOperations: azureTopic.EnableBatchedOperations,
                EnablePartitioning: azureTopic.EnablePartitioning,
                RequiresDuplicateDetection: azureTopic.RequiresDuplicateDetection,
                SupportOrdering: azureTopic.SupportOrdering,
                AutoDeleteOnIdle: azureTopic.AutoDeleteOnIdle,
                DefaultMessageTimeToLive: azureTopic.DefaultMessageTimeToLive,
                DuplicateDetectionHistoryTimeWindow: azureTopic.DuplicateDetectionHistoryTimeWindow,
                Subscriptions: azureSubscriptions.Select(subscription =>
                    SubscriptionMapper.Map(
                        subscription,
                        azureSubscriptionRuntimes.Single(runtime =>
                            runtime.TopicName == azureTopic.Name &&
                            runtime.SubscriptionName == subscription.SubscriptionName),
                        azureTopicRuntime)
                    ).ToArray()
            );
    }
}
