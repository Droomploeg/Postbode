using Droomploeg.DreamOps.Domain.ServiceBus.Models;

namespace Droomploeg.DreamOps.Application.ServiceBus.Services;

/// <summary>
/// Runtime information service interface.
/// </summary>
public interface IRuntimeInfoService
{
    /// <summary>
    /// Get all entities of type <see cref="IEntity"/> from cache.
    /// </summary>
    /// <typeparam name="TEntity">Entity type of type <see cref="IEntity"/></typeparam>
    /// <returns>Array of <see cref="IEntity"/></returns>
    Task<TEntity[]> GetAllAsync<TEntity>() where TEntity : IEntity;

    /// <summary>
    /// Get all queues.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="IEnumerable{T}"/> of <see cref="Queue"/></returns>
    Task<ICollection<Queue>> GetAllQueuesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all topics.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="IEnumerable{T}"/> of <see cref="Topic"/></returns>
    Task<ICollection<Topic>> GetAllTopicsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get <see cref="Queue"/> by name.
    /// </summary>
    /// <param name="name">Name of the <see cref="Queue"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Queue"/> if found else null</returns>
    Task<Queue?> GetQueueByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get <see cref="Topic"/> by name.
    /// </summary>
    /// <param name="name">Name of the <see cref="Topic"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Topic"/> if found else null</returns>
    Task<Topic?> GetTopicByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get <see cref="Subscription"/> by topic and name.
    /// </summary>
    /// <param name="topicName">Name of the topic</param>
    /// <param name="subscriptionName">Name of the subscription</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Subscription"/> if found else null</returns>
    Task<Subscription?> GetSubscriptionAsync(string topicName, string subscriptionName, CancellationToken cancellationToken = default);
}
