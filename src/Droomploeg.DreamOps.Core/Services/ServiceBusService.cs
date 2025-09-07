using Droomploeg.DreamOps.Core.Models;
using Droomploeg.DreamOps.Core.Repositories;

namespace Droomploeg.DreamOps.Core.Services;

/// <summary>
/// ServiceBusService class. For getting ServiceBus information.
/// </summary>
/// <param name="repository"></param>
public class ServiceBusService(IServiceBusRepository repository)
{
    private readonly IServiceBusRepository _repository = repository;

    /// <summary>
    /// Get all <see cref="IEntity"/> from cache.
    /// </summary>
    /// <typeparam name="T"><see cref="IEntity"/></typeparam>
    /// <returns><see cref="Array"/> of <see cref="IEntity"/></returns>
    public async Task<T[]> GetAllAsync<T>()
        where T : IEntity
    {
        var entities = await _repository.GetAllEntitiesAsync();
        return [.. entities
            .Where(e => e is T)
            .Cast<T>()];
    }

    /// <summary>
    /// Get all queues.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see cref="IEnumerable{T}"/> of <see cref="Queue"/></returns>
    public async Task<IEnumerable<Queue>> GetAllQueuesAsync(CancellationToken cancellationToken = default)
        => await _repository.GetAllQueuesAsync(cancellationToken);

    /// <summary>
    /// Get all topics.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see cref="IEnumerable{T}"/> of <see cref="Topic"/></returns>
    public async Task<IEnumerable<Topic>> GetAllTopicsAsync(CancellationToken cancellationToken = default)
        => await _repository.GetAllTopicsAsync(cancellationToken);

    /// <summary>
    /// Get queue by name.
    /// </summary>
    /// <param name="name">Name of the queue</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see cref="Queue"/></returns>
    public async Task<Queue?> GetQueueByNameAsync(string name, CancellationToken cancellationToken = default)
        => await _repository.GetQueueAsync(name, cancellationToken);

    /// <summary>
    /// Get subscription by topic and subscription name.
    /// </summary>
    /// <param name="topicName">Name of the topic</param>
    /// <param name="subscriptionName">Name of the subscription</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see cref="Subscription"/></returns>
    public async Task<Subscription?> GetSubscriptionAsync(string topicName, string subscriptionName, CancellationToken cancellationToken = default)
        => await _repository.GetSubscriptionAsync(topicName, subscriptionName);

    /// <summary>
    /// Get topic by name.
    /// </summary>
    /// <param name="name">Name of the topic</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see cref="Array"/> of <see cref="IEntity"/></returns>
    public async Task<Topic?> GetTopicByNameAsync(string name, CancellationToken cancellationToken = default)
        => await _repository.GetTopicAsync(name, cancellationToken);
}
