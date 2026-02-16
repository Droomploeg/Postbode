using Azure.Messaging.ServiceBus;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Extensions;

internal static class ServiceBusReceiverExtensions
{
    internal static async Task ResubmitNumberOfMessagesAsync(this ServiceBusReceiver receiver, 
        ServiceBusSender sender, 
        DateTimeOffset timestamp, 
        bool generateNewMessageIds,
        CancellationToken cancellationToken = default)
    {
        var resubmitKey = Guid.NewGuid();

        var resubmit = true;
        while (resubmit)
        {
            var receivedMessageList = await receiver.PeekMessagesAsync(ServiceBusConstants.BucketSize, cancellationToken: cancellationToken);
            var receivedMessagesToResubmitList = receivedMessageList
                .Where(m => m.EnqueuedTime < timestamp && !m.IsAlreadyResubmitted(resubmitKey))
                .ToList();

            if (receivedMessagesToResubmitList.Count == 0)
            {
                return;
            }

            var sendBatch = await sender.CreateMessageBatchAsync(cancellationToken);
            foreach (var receivedMessageToSubmit in receivedMessagesToResubmitList)
            {
                var servicebusMessage = receivedMessageToSubmit.CopyToServiceBusMessage();
                if (generateNewMessageIds)
                {
                    servicebusMessage.MessageId = Guid.NewGuid().ToString();
                }

                servicebusMessage.ApplicationProperties[ServiceBusConstants.ResubmitKey] = resubmitKey;
                if (!sendBatch.TryAddMessage(servicebusMessage))
                {
                    await sender.SendMessagesAsync(sendBatch, cancellationToken);

                    sendBatch = await sender.CreateMessageBatchAsync(cancellationToken);
                    if (!sendBatch.TryAddMessage(servicebusMessage))
                    {
                        throw new Exception("Message is too large to fit in a batch");
                    }
                }
            }

            await sender.SendMessagesAsync(sendBatch, cancellationToken);
            resubmit = receivedMessagesToResubmitList.Count > 0 ||
                receivedMessageList.Count == receivedMessagesToResubmitList.Count;
        }
    }

    internal static async Task ResubmitNumberOfMessagesWithDeleteAsync(this ServiceBusReceiver receiver, 
        ServiceBusSender sender,
        DateTimeOffset timestamp, 
        bool generateNewMessageIds,
        CancellationToken cancellationToken = default)
    {
        var resubmitKey = Guid.NewGuid();

        var lockMessagesInBatch = new List<ServiceBusReceivedMessage>();
        var canResubmit = true;
        while (canResubmit)
        {
            var receivedMessageList = await receiver.LockMessagesAsync(ServiceBusConstants.BucketSize, cancellationToken);
            var receivedMessagesToResubmitList = receivedMessageList
                .Where(m => m.EnqueuedTime < timestamp && !m.IsAlreadyResubmitted(resubmitKey))
                .ToList();
            if (receivedMessagesToResubmitList.Count == 0)
            {
                return;
            }

            var sendBatch = await sender.CreateMessageBatchAsync(cancellationToken);
            foreach (var receivedMessageToSubmit in receivedMessagesToResubmitList)
            {
                var servicebusMessage = receivedMessageToSubmit.CopyToServiceBusMessage();
                if (generateNewMessageIds)
                {
                    servicebusMessage.MessageId = Guid.NewGuid().ToString();
                }

                servicebusMessage.ApplicationProperties[ServiceBusConstants.ResubmitKey] = resubmitKey;
                if (!sendBatch.TryAddMessage(servicebusMessage))
                {
                    await sender.SendMessagesAsync(sendBatch, cancellationToken);
                    foreach (var message in lockMessagesInBatch)
                    {
                        await receiver.CompleteMessageAsync(message, cancellationToken);
                    }

                    sendBatch = await sender.CreateMessageBatchAsync(cancellationToken);
                    if (!sendBatch.TryAddMessage(servicebusMessage))
                    {
                        throw new Exception("Message is too large to fit in a batch");
                    }
                    lockMessagesInBatch.Add(receivedMessageToSubmit);
                }
                else
                {
                    lockMessagesInBatch.Add(receivedMessageToSubmit);
                }
            }

            if (sendBatch.Count > 0)
            {
                await sender.SendMessagesAsync(sendBatch, cancellationToken);
                foreach (var message in lockMessagesInBatch)
                {
                    await receiver.CompleteMessageAsync(message, cancellationToken);
                }
            }

            await sender.SendMessagesAsync(sendBatch, cancellationToken);
            
            canResubmit = receivedMessagesToResubmitList.Count > 0 ||
                receivedMessageList.Count == receivedMessagesToResubmitList.Count;
        }
    }



    internal static async Task<bool> SearchAndResubmitAsync(this ServiceBusReceiver receiver, 
        ServiceBusSender sender,
        ServiceBusReceivedMessage receivedMessage, 
        ServiceBusMessage sendMessage,
        long searchBucketSize, 
        ResubmitOptions options, 
        CancellationToken cancellationToken = default)
    {
        var result = false;
        var lockedMessages = await receiver.LockMessagesAsync(searchBucketSize, cancellationToken);
        var resubmitMessage = lockedMessages.FirstOrDefault(m => m.Compare(receivedMessage));
        if (resubmitMessage is not null)
        {
            if (options.GenerateMessageIds)
            {
                sendMessage.MessageId = Guid.NewGuid().ToString();
            }
            await sender.SendMessageAsync(sendMessage, cancellationToken);

            if (options.DeleteMessage)
            {
                lockedMessages.Remove(resubmitMessage);
                await receiver.CompleteMessageAsync(resubmitMessage, cancellationToken);
            }
            result = true;
        }
        await receiver.UnlockMessagesAsync(lockedMessages, cancellationToken);

        return result;
    }

    internal static async Task<bool> SearchAndCompleteAsync(this ServiceBusReceiver receiver,
        ServiceBusReceivedMessage receivedMessage,
        long searchBucketSize,
        CancellationToken cancellationToken = default)
    {
        var result = false;
        var lockedMessages = await receiver.LockMessagesAsync(searchBucketSize, cancellationToken);
        var completeMessage = lockedMessages.FirstOrDefault(m => m.Compare(receivedMessage));
        if (completeMessage is not null)
        {
            lockedMessages.Remove(completeMessage);

            await receiver.CompleteMessageAsync(completeMessage, cancellationToken);
            result = true;
        }

        await receiver.UnlockMessagesAsync(lockedMessages, cancellationToken);
        return result;
    }
    
    internal static async Task CompleteMessagesAsync(this ServiceBusReceiver receiver, DateTimeOffset dateTime, CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var messages = await receiver.PeekMessagesAsync(ServiceBusConstants.BucketSize, cancellationToken: cancellationToken);
            var numberOfMessages = messages.Count(m => m.EnqueuedTime < dateTime);

            if (numberOfMessages < 1)
            {
                return;
            }

            var receivedMessages = await receiver.ReceiveMessagesAsync(numberOfMessages, cancellationToken: cancellationToken);

            var tasks = receivedMessages.Select(message => receiver.CompleteMessageAsync(message, cancellationToken)).ToList();
            await Task.WhenAll(tasks);
        }
    }

    private static async Task<List<ServiceBusReceivedMessage>> LockMessagesAsync(this ServiceBusReceiver receiver,
        long numberOfMessagesToReceive, CancellationToken cancellationToken = default)
    {
        var calls = (int)Math.Ceiling((double)numberOfMessagesToReceive / ServiceBusConstants.BucketSize);

        Task<IReadOnlyList<ServiceBusReceivedMessage>> receiveMessageCalls = receiver.ReceiveMessagesAsync(ServiceBusConstants.BucketSize, cancellationToken: cancellationToken);
        
        var tasks = Enumerable.Range(0, calls)
            .Select(i => receiveMessageCalls).ToList();

        await Task.WhenAll(tasks);

        return tasks
            .SelectMany(m => m.Result)
            .ToList();
    }

    private static Task UnlockMessagesAsync(this ServiceBusReceiver receiver,
        ICollection<ServiceBusReceivedMessage> messages, CancellationToken cancellationToken = default)
    {
        var tasks = messages.Select(m => receiver.AbandonMessageAsync(m, cancellationToken: cancellationToken)).ToList();
        return Task.WhenAll(tasks);
    }

}
