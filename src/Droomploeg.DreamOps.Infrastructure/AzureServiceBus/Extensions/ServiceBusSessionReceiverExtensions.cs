using Azure.Messaging.ServiceBus;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Extensions;

internal static class ServiceBusSessionReceiverExtensions
{
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

