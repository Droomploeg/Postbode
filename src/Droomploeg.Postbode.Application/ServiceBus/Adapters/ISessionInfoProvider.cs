namespace Droomploeg.Postbode.Application.ServiceBus.Adapters;

/// <summary>
/// Provides session information for Service Bus entities.
/// Used by adapters to determine whether a queue or subscription requires session-aware receivers.
/// </summary>
public interface ISessionInfoProvider
{
    /// <summary>
    /// Determines whether the specified queue requires sessions.
    /// </summary>
    /// <param name="queue">The name of the queue.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see langword="true"/> if the queue requires sessions; otherwise, <see langword="false"/>.</returns>
    Task<bool> RequiresSessionAsync(string queue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the specified topic subscription requires sessions.
    /// </summary>
    /// <param name="topic">The name of the topic.</param>
    /// <param name="subscription">The name of the subscription.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see langword="true"/> if the subscription requires sessions; otherwise, <see langword="false"/>.</returns>
    Task<bool> RequiresSessionAsync(string topic, string subscription, CancellationToken cancellationToken = default);
}
