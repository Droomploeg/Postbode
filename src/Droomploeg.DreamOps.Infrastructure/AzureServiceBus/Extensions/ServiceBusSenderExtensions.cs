using Azure.Messaging.ServiceBus;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Extensions;

internal static class ServiceBusSenderExtensions
{
    internal static async Task SendBulkMessageAsync(this ServiceBusSender sender, IEnumerable<ServiceBusMessage> messages,
        CancellationToken cancellationToken = default)
    {
        var messsageArray = messages.ToArray();
        var batch = await sender.CreateMessageBatchAsync(cancellationToken);
        foreach (var message in messsageArray)
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
