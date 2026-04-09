using System.Diagnostics.CodeAnalysis;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Application.ServiceBus.Services;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Droomploeg.DreamOps.Infrastructure.Contexts;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Services;

/// <summary>
/// Implementation of <see cref="IRuntimeInfoService"/>
/// </summary>
[ExcludeFromCodeCoverage( Justification = "This class is responsible for retrieving runtime information from Azure Service Bus, which is a critical part of the application's infrastructure. Testing this class would require extensive setup and may not provide significant value in terms of code coverage.")]
public class RuntimeInfoService : IRuntimeInfoService
{
    private readonly IRuntimeInfoAdapter _adapter;
    private readonly IContextSetter _contextSetter;

    /// <summary>
    /// Initializes a new instance of <see cref="RuntimeInfoService"/>.
    /// </summary>
    /// <param name="adapter"><see cref="IRuntimeInfoAdapter"/></param>
    /// <param name="contextSetter"><see cref="IContextSetter"/></param>
    public RuntimeInfoService(IRuntimeInfoAdapter adapter, IContextSetter contextSetter)
    {
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _contextSetter = contextSetter ?? throw new ArgumentNullException(nameof(contextSetter));
    }

    /// <inheritdoc cref="IRuntimeInfoService.GetAllAsync{TEntity}"/>
    public async Task<T[]> GetAllAsync<T>()
        where T : IEntity
    {
        await _contextSetter.GetAndUpdateAsync();

        var entities = await _adapter.GetAllEntitiesAsync();
        return [.. entities.OfType<T>()];
    }

    /// <inheritdoc cref="IRuntimeInfoService.GetAllQueuesAsync(CancellationToken)"/>
    public async Task<ICollection<Queue>> GetAllQueuesAsync(CancellationToken cancellationToken = default)
    {
        await _contextSetter.GetAndUpdateAsync();
        return await _adapter.GetAllQueuesAsync(cancellationToken);
    }

    /// <inheritdoc cref="IRuntimeInfoService.GetAllTopicsAsync(CancellationToken)"/>
    public async Task<ICollection<Topic>> GetAllTopicsAsync(CancellationToken cancellationToken = default)
    {
        await _contextSetter.GetAndUpdateAsync();
        return await _adapter.GetAllTopicsAsync(cancellationToken);
    }

    /// <inheritdoc cref="IRuntimeInfoService.GetQueueByNameAsync(string, CancellationToken)"/>
    public async Task<Queue?> GetQueueByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        await _contextSetter.GetAndUpdateAsync();
        return await _adapter.GetQueueAsync(name, cancellationToken);
    }

    /// <inheritdoc cref="IRuntimeInfoService.GetTopicByNameAsync(string, CancellationToken)"/>
    public async Task<Topic?> GetTopicByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        await _contextSetter.GetAndUpdateAsync();
        return await _adapter.GetTopicAsync(name, cancellationToken);
    }

    /// <inheritdoc cref="IRuntimeInfoService.GetSubscriptionAsync(string, string, CancellationToken)"/>
    public async Task<Subscription?> GetSubscriptionAsync(string topicName, string subscriptionName, CancellationToken cancellationToken = default)
    {
        await _contextSetter.GetAndUpdateAsync();
        return await _adapter.GetSubscriptionAsync(topicName, subscriptionName, cancellationToken);
    }
}

