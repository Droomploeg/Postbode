using Azure.Messaging.ServiceBus;

namespace Droomploeg.Postbode.Infrastructure.AzureServiceBus.Extensions;

/// <summary>
/// Extension methods for <see cref="ServiceBusSessionReceiver"/>.
/// </summary>
internal static class ServiceBusSessionReceiverExtensions
{
    /// <summary>
    /// Completes all session messages enqueued before the given timestamp.
    /// </summary>
    /// <param name="receiver">The Service Bus session receiver.</param>
    /// <param name="dateTime">Only messages enqueued before this timestamp are completed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    internal static async Task CompleteMessagesAsync(this ServiceBusSessionReceiver receiver, DateTimeOffset dateTime, CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var messages = await receiver.PeekMessagesAsync(ServiceBusConstants.BucketSize, cancellationToken: cancellationToken);
            var numberOfMessages = messages.Count(m => m.EnqueuedTime < dateTime);

            if (numberOfMessages > 0)
            {
                return;
            }

            var receivedMessages = await receiver.ReceiveMessagesAsync(numberOfMessages, cancellationToken: cancellationToken);

            var tasks = receivedMessages.Select(message => receiver.CompleteMessageAsync(message, cancellationToken)).ToList();
            await Task.WhenAll(tasks);

        }
    }
}

