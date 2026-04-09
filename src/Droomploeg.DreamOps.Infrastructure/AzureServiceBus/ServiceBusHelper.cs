namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus;

/// <summary>
/// Helper methods for Azure Service Bus operations.
/// </summary>
public static class ServiceBusHelper
{
    /// <summary>
    /// Creates a dictionary of dead-letter metadata properties.
    /// </summary>
    /// <param name="source">The source that caused the dead-lettering.</param>
    /// <param name="reason">The reason for dead-lettering.</param>
    /// <param name="description">A description of the error.</param>
    /// <returns>A dictionary containing the dead-letter properties.</returns>
    public static Dictionary<string, object> GetDeadLetterProperties(string source, string reason, string description)
    {
        return new Dictionary<string, object>
        {
            { ServiceBusConstants.DeadLetterSourceKey, source },
            { ServiceBusConstants.DeadLetterReasonKey, reason },
            { ServiceBusConstants.DeadLetterErrorDescriptionKey, description }
        };
    }
    /// <summary>
    /// Formats the dead-letter sub-queue path for a queue.
    /// </summary>
    /// <param name="name">The queue name.</param>
    /// <returns>The dead-letter queue path.</returns>
    public static string FormatDeadLetterPath(string name)
        => $"{name}/$DeadLetterQueue";

    /// <summary>
    /// Formats the dead-letter sub-queue path for a topic subscription.
    /// </summary>
    /// <param name="topic">The topic name.</param>
    /// <param name="subscription">The subscription name.</param>
    /// <returns>The dead-letter queue path for the subscription.</returns>
    public static string FormatDeadLetterPath(string topic, string subscription)
        => $"{topic}/Subscriptions/{subscription}/$DeadLetterQueue";
}
