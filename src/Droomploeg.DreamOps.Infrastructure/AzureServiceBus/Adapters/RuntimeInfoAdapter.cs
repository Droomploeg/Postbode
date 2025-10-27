using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Application;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Mappers;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Extensions;
using Microsoft.Extensions.Azure;
using Droomploeg.DreamOps.Infrastructure.Contexts;
using Azure.Messaging.ServiceBus;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Adapters;

public class RuntimeInfoAdapter : IRuntimeInfoAdapter
{
    private readonly ApplicationContext _context;
    private readonly IAzureClientFactory<ServiceBusClient> _clientFactory;
    private readonly IAzureClientFactory<ServiceBusAdministrationClient> _adminClientFactory;

    public RuntimeInfoAdapter(
        ApplicationContext context,
        IAzureClientFactory<ServiceBusClient> clientFactory,
        IAzureClientFactory<ServiceBusAdministrationClient> adminClientFactory)
    {
        _adminClientFactory = adminClientFactory ?? throw new ArgumentNullException(nameof(adminClientFactory));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
    }

    public async Task<ICollection<IEntity>> GetAllEntitiesAsync(CancellationToken cancellationToken = default)
    {
        var entities = new List<IEntity>();

        var queueTask = GetAllQueuesAsync(cancellationToken);
        var topicTask = GetAllTopicsAsync(cancellationToken);

        await Task.WhenAll([queueTask, topicTask]);

        entities.AddRange(queueTask.Result);
        entities.AddRange(topicTask.Result);

        return entities;
    }

    public async Task<Queue?> GetQueueAsync(string queue, CancellationToken cancellationToken = default)
    {
        var adminClient = _adminClientFactory.CreateClient(_context);
        var azureQueueResponse = await adminClient.GetQueueAsync(queue, cancellationToken);
        var azureQueue = azureQueueResponse.Value;

        var azureQueueRuntimeResponse = await adminClient.GetQueueRuntimePropertiesAsync(azureQueue.Name, cancellationToken);
        var azureQueueRuntime = azureQueueRuntimeResponse.Value;

        return QueueMapper.Map(azureQueue, azureQueueRuntime);
    }

    public async Task<Topic?> GetTopicAsync(string topic, CancellationToken cancellationToken = default)
    {
        var adminClient = _adminClientFactory.CreateClient(_context);
        var azureTopicResponse = await adminClient.GetTopicAsync(topic, cancellationToken);
        var azureTopic = azureTopicResponse.Value;
        var azureTopicRuntimePropertiesResponse = await adminClient.GetTopicRuntimePropertiesAsync(azureTopic.Name, cancellationToken);
        var azureTopicRuntime = azureTopicRuntimePropertiesResponse.Value;

        var azureSubscriptionsResponse = adminClient
            .GetSubscriptionsAsync(azureTopic.Name, cancellationToken)
            .GetAsyncEnumerator(cancellationToken);

        var azureSubscriptions = new List<SubscriptionProperties>();
        var azureSubscriptionRuntimes = new List<SubscriptionRuntimeProperties>();

        while (await azureSubscriptionsResponse.MoveNextAsync())
        {
            var azureSubscription = azureSubscriptionsResponse.Current;

            var azureSubscriptionRuntimeResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(
                azureTopic.Name,
                azureSubscription.SubscriptionName,
                cancellationToken);
            var azureSubscriptionRuntime = azureSubscriptionRuntimeResponse.Value;

            azureSubscriptions.Add(azureSubscription);
            azureSubscriptionRuntimes.Add(azureSubscriptionRuntime);
        }

        return TopicMapper.Map(azureTopic, azureTopicRuntime, azureSubscriptions, azureSubscriptionRuntimes);
    }

    public async Task<Subscription?> GetSubscriptionAsync(string topic, string subscription, CancellationToken cancellationToken = default)
    {
        var adminClient = _adminClientFactory.CreateClient(_context);
        var azureSubscriptionResponse = await adminClient.GetSubscriptionAsync(topic, subscription, cancellationToken);
        var azureSubscription = azureSubscriptionResponse.Value;

        var azureTopicRuntimeResponse = await adminClient.GetTopicRuntimePropertiesAsync(topic, cancellationToken);
        var azureTopicRuntime = azureTopicRuntimeResponse.Value;

        var azureSubscriptionRuntimeResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(
            topic,
            subscription,
            cancellationToken);
        var azureSubscriptionRuntime = azureSubscriptionRuntimeResponse.Value;

        return SubscriptionMapper.Map(azureSubscription, azureSubscriptionRuntime, azureTopicRuntime);
    }

    public async Task<ICollection<Queue>> GetAllQueuesAsync(CancellationToken cancellationToken = default)
    {
        var adminClient = _adminClientFactory.CreateClient(_context);
        var queues = new BlockingCollection<Queue>();
        var azureQueuesResponse = adminClient
            .GetQueuesAsync(cancellationToken)
            .GetAsyncEnumerator(cancellationToken);

        while (await azureQueuesResponse.MoveNextAsync())
        {
            var azureQueue = azureQueuesResponse.Current;

            var azureQueueRuntimeResponse = await adminClient.GetQueueRuntimePropertiesAsync(azureQueue.Name, cancellationToken);
            var azureQueueRuntime = azureQueueRuntimeResponse.Value;

            queues.Add(QueueMapper.Map(azureQueue, azureQueueRuntime), cancellationToken);
        }

        return [.. queues];
    }

    public async Task<ICollection<Topic>> GetAllTopicsAsync(CancellationToken cancellationToken = default)
    {
        var topics = new List<Topic>();

        var adminClient = _adminClientFactory.CreateClient(_context);
        var azureTopicsReponse = adminClient.GetTopicsAsync(cancellationToken)
            .GetAsyncEnumerator(cancellationToken);

        while (await azureTopicsReponse.MoveNextAsync())
        {
            var azureTopic = azureTopicsReponse.Current;
            var azureTopicRuntimeResponse = await adminClient.GetTopicRuntimePropertiesAsync(azureTopic.Name, cancellationToken);
            var azureTopicRuntime = azureTopicRuntimeResponse.Value;

            var azureSubscriptions = new List<SubscriptionProperties>();
            var azureSubscriptionRuntimes = new List<SubscriptionRuntimeProperties>();

            var azureSubscriptionsResponse = adminClient
                .GetSubscriptionsAsync(azureTopic.Name, cancellationToken)
                .GetAsyncEnumerator(cancellationToken);
            while (await azureSubscriptionsResponse.MoveNextAsync())
            {
                var azureSubscription = azureSubscriptionsResponse.Current;
                var azureSubscriptionRuntimeResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(
                    azureTopic.Name,
                    azureSubscription.SubscriptionName,
                    cancellationToken);

                var azureSubscriptionRuntime = azureSubscriptionRuntimeResponse.Value;

                azureSubscriptions.Add(azureSubscription);
                azureSubscriptionRuntimes.Add(azureSubscriptionRuntime);
            }

            topics.Add(TopicMapper.Map(azureTopic, azureTopicRuntime, azureSubscriptions, azureSubscriptionRuntimes));
        }

        return topics;
    }
}
