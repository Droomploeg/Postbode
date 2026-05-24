using Azure.Messaging.ServiceBus;

namespace Droomploeg.Postbode.Infrastructure.AzureServiceBus.Extensions;

/// <summary>
/// Extension methods for <see cref="ServiceBusSender"/>.
/// </summary>
internal static class ServiceBusSenderExtensions
{
    /// <summary>
    /// Sends a collection of messages in batches, automatically splitting when a batch is full.
    /// </summary>
    /// <param name="sender">The Service Bus sender.</param>
    /// <param name="messages">The messages to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="InvalidOperationException">Thrown when a single message is too large to fit in a batch.</exception>
    internal static async Task SendBulkMessageAsync(this ServiceBusSender sender, ICollection<ServiceBusMessage> messages,
        CancellationToken cancellationToken = default)
    {
        var messageArray = messages.ToArray();
        var batch = await sender.CreateMessageBatchAsync(cancellationToken);
        foreach (var message in messageArray)
        {
            if (!batch.TryAddMessage(message))
            {
                await sender.SendMessagesAsync(batch, cancellationToken);
                batch = await sender.CreateMessageBatchAsync(cancellationToken);
                if (!batch.TryAddMessage(message))
                {
                    throw new InvalidOperationException("Message is too large to fit in a batch");
                }
            }
        }
        await sender.SendMessagesAsync(batch, cancellationToken);
    }
}
