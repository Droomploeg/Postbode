using Droomploeg.DreamOps.Core.Models;

namespace Droomploeg.DreamOps.Core.Repositories;

/// <summary>
/// Servicebus repository interface.
/// </summary>
public interface IServiceBusRepository
{
    /// <summary>
    /// Get all entities.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"></param>
    /// <returns><see cref="IEnumerable{T}"/> of <see cref="IEntity"/></returns>
    Task<IEnumerable<IEntity>> GetAllEntitiesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all queues.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"></param>
    /// <returns><see cref="IEnumerable{T}"/> of <see cref="Queue"/></returns>
    Task<IEnumerable<Queue>> GetAllQueuesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get queue by name.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"></param>
    /// <returns><see cref="IEnumerable{T}"/> of <see cref="Queue"/></returns>
    Task<Queue> GetQueueAsync(string queue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all topics with subscriptions.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"></param>
    /// <returns><see cref="IEnumerable{T}"/> of <see cref="Topic"/></returns>
    Task<IEnumerable<Topic>> GetAllTopicsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get topic by name.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"></param>
    /// <returns><see cref="IEnumerable{T}"/> of <see cref="Topic"/></returns>
    Task<Topic> GetTopicAsync(string topic, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get subscription.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"></param>
    /// <returns><see cref="IEnumerable{T}"/> of <see cref="Subscription"/></returns>
    Task<Subscription> GetSubscriptionAsync(string topic, string subscription, CancellationToken cancellationToken = default);
}
