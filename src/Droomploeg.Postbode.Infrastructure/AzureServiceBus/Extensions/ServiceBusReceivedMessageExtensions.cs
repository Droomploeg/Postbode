using Azure.Messaging.ServiceBus;

namespace Droomploeg.Postbode.Infrastructure.AzureServiceBus.Extensions;

/// <summary>
/// Extension methods for <see cref="ServiceBusReceivedMessage"/>.
/// </summary>
internal static class ServiceBusReceivedMessageExtensions
{
    /// <summary>
    /// Copies a received message to a new <see cref="ServiceBusMessage"/> suitable for resending.
    /// </summary>
    /// <param name="receivedMessage">The received message to copy.</param>
    /// <returns>A new <see cref="ServiceBusMessage"/> with the same body, properties, and application properties.</returns>
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

    /// <summary>
    /// Compares two received messages by message ID and sequence number.
    /// </summary>
    /// <param name="message">The first message.</param>
    /// <param name="messageToCompare">The second message to compare against.</param>
    /// <returns><see langword="true"/> if both messages have the same message ID and sequence number; otherwise <see langword="false"/>.</returns>
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
            message.SequenceNumber == messageToCompare.SequenceNumber;
    }


    /// <summary>
    /// Checks whether a message has already been resubmitted using the given resubmitted key.
    /// </summary>
    /// <param name="message">The received message to check.</param>
    /// <param name="resubmitKey">The resubmitted key to compare against.</param>
    /// <returns><see langword="true"/> if the message contains a matching resubmitted key; otherwise <see langword="false"/>.</returns>
    internal static bool IsAlreadyResubmitted(this ServiceBusReceivedMessage message, Guid resubmitKey)
    {
        if (message.ApplicationProperties.TryGetValue(ServiceBusConstants.ResubmitKey, out var compareResubmitKey))
        {
            return compareResubmitKey is Guid resubmitKeyGuid && resubmitKeyGuid == resubmitKey;
        }

        return false;
    }
}
