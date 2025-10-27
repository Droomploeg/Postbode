using Azure.Messaging.ServiceBus;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Extensions;

internal static class ServiceBusReceivedMessageExtensions
{
    internal static ServiceBusMessage CopyToServiceBusMessage(this ServiceBusReceivedMessage receivedMessage)
    {
        var sendMessage = new ServiceBusMessage(receivedMessage.Body)
        {
            ContentType = receivedMessage.ContentType,
            Subject = receivedMessage.Subject,
            MessageId = receivedMessage.MessageId,
            SessionId = receivedMessage.SessionId,
            ReplyToSessionId = receivedMessage.ReplyToSessionId,
            CorrelationId = receivedMessage.CorrelationId,
            PartitionKey = receivedMessage.PartitionKey,
            ReplyTo = receivedMessage.ReplyTo
        };

        sendMessage.ReplyToSessionId = receivedMessage.ReplyToSessionId;
        sendMessage.To = receivedMessage.To;
        sendMessage.TransactionPartitionKey = receivedMessage.TransactionPartitionKey;

        foreach (var kvp in receivedMessage.ApplicationProperties)
        {
            sendMessage.ApplicationProperties.Add(kvp.Key, kvp.Value);
        }

        return sendMessage;
    }

    internal static bool Compare(this ServiceBusReceivedMessage? message, ServiceBusReceivedMessage? messageToCompare)
    { 
        if (message == null && messageToCompare == null)
        {
            return true;
        }

        if (message == null || messageToCompare == null)
        {
            return false;
        }

        return message.MessageId.Equals(messageToCompare.MessageId) &&
            message.SequenceNumber == messageToCompare.SequenceNumber &&
            message.EnqueuedTime == messageToCompare.EnqueuedTime;
    }


    internal static bool IsAlreadyResubmitted(this ServiceBusReceivedMessage message, Guid resubmitKey)
    {
        if (message.ApplicationProperties.TryGetValue(ServiceBusConstants.ResubmitKey, out var compareResubmitKey))
        {
            return compareResubmitKey is Guid resubmitKeyGuid && resubmitKeyGuid == resubmitKey;
        }

        return false;
    }
}
