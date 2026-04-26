using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Extensions;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Mappers;
using Droomploeg.DreamOps.Infrastructure.Contexts;
using Microsoft.Extensions.Azure;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Adapters;

/// <summary>Adapter for retrieving Service Bus runtime information using Azure Service Bus.</summary>
[ExcludeFromCodeCoverage( Justification = "This class is responsible for retrieving runtime information from Azure Service Bus, which is a critical part of the application's infrastructure. Testing this class would require extensive setup and may not provide significant value in terms of code coverage.")]
public class RuntimeInfoAdapter : IRuntimeInfoAdapter
{
    private readonly ApplicationContext _context;
    private readonly IAzureClientFactory<ServiceBusAdministrationClient> _adminClientFactory;

    /// <summary>Initializes a new instance of <see cref="RuntimeInfoAdapter"/>.</summary>
    /// <param name="context"><see cref="ApplicationContext"/> for the current request.</param>
    /// <param name="adminClientFactory">Factory for creating <see cref="ServiceBusAdministrationClient"/> instances.</param>
    public RuntimeInfoAdapter(
        ApplicationContext context,
        IAzureClientFactory<ServiceBusAdministrationClient> adminClientFactory)
    {
        _adminClientFactory = adminClientFactory ?? throw new ArgumentNullException(nameof(adminClientFactory));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<Queue?> GetQueueAsync(string queue, CancellationToken cancellationToken = default)
    {
        var adminClient = _adminClientFactory.CreateClient(_context);
        var azureQueueResponse = await adminClient.GetQueueAsync(queue, cancellationToken);
        var azureQueue = azureQueueResponse.Value;

        var azureQueueRuntimeResponse = await adminClient.GetQueueRuntimePropertiesAsync(azureQueue.Name, cancellationToken);
        var azureQueueRuntime = azureQueueRuntimeResponse.Value;

        return QueueMapper.Map(azureQueue, azureQueueRuntime);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<ICollection<Topic>> GetAllTopicsAsync(CancellationToken cancellationToken = default)
    {
        var topics = new List<Topic>();

        var adminClient = _adminClientFactory.CreateClient(_context);
        var azureTopicsResponse = adminClient.GetTopicsAsync(cancellationToken)
            .GetAsyncEnumerator(cancellationToken);

        while (await azureTopicsResponse.MoveNextAsync())
        {
            var azureTopic = azureTopicsResponse.Current;
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
