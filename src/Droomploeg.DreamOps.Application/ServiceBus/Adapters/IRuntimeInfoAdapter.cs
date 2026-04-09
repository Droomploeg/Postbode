using Droomploeg.DreamOps.Domain.ServiceBus.Models;

namespace Droomploeg.DreamOps.Application.ServiceBus.Adapters;

/// <summary>
/// Runtime information adapter interface.
/// </summary>
public interface IRuntimeInfoAdapter
{
    /// <summary>
    /// Get all entities.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="ICollection{T}"/> of <see cref="IEntity"/></returns>
    Task<ICollection<IEntity>> GetAllEntitiesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all queues.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="ICollection{T}"/> of <see cref="Queue"/></returns>
    Task<ICollection<Queue>> GetAllQueuesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get queue by name.
    /// </summary>
    /// <param name="queue">Name of the queue</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Queue"/></returns>
    Task<Queue?> GetQueueAsync(string queue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all topics with subscriptions.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="ICollection{T}"/> of <see cref="Topic"/></returns>
    Task<ICollection<Topic>> GetAllTopicsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get topic by name.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Topic"/></returns>
    Task<Topic?> GetTopicAsync(string topic, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get subscription.
    /// </summary>
    /// <param name="topic">Name of the topic</param>
    /// <param name="subscription">Name of the subscription</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Subscription"/></returns>
    Task<Subscription?> GetSubscriptionAsync(string topic, string subscription, CancellationToken cancellationToken = default);
}
