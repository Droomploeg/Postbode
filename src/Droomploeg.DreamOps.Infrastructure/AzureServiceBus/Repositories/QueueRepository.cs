using System.Threading;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Core;
using Droomploeg.DreamOps.Core.Models;
using Droomploeg.DreamOps.Core.Repositories;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Extensions;
using Microsoft.Extensions.Azure;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Repositories;

public class QueueRepository(
    TimeProvider timeProvider,
    IServiceBusClientContext context,
    IAzureClientFactory<ServiceBusAdministrationClient> adminClientFactory,
    IAzureClientFactory<ServiceBusClient> clientFactory) : IQueueRepository<ServiceBusMessage, ServiceBusReceivedMessage>
{
    #region [Active Queue]

    public async Task SendAsync(string queue, ICollection<ServiceBusMessage> messages, CancellationToken cancellationToken = default)
    {
        var client = clientFactory.CreateClient(context.CurrentClient);
        var sender = client
            .CreateSender(queue);

        await sender.SendBulkMessageAsync(messages, cancellationToken);
        await sender.CloseAsync(cancellationToken);
    }

    public async Task<IEnumerable<ServiceBusReceivedMessage>> PeekActiveMessagesAsync(string queue,
        long fromSequenceNumber = RepositoryConstants.DefaultStartIndex,
        int numberOfMessages = RepositoryConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default)
    {
        var adminClient = adminClientFactory.CreateClient(context.CurrentClient);
        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        if (fromSequenceNumber > azureQueueRuntimeProperties.ActiveMessageCount)
        {
            return [];
        }

        var client = clientFactory.CreateClient(context.CurrentClient);
        var receiver = client.CreateReceiver(queue, ServiceBusConstants.PeekLockOptions);
        var messages = await receiver.PeekMessagesAsync(numberOfMessages, fromSequenceNumber, cancellationToken);
        await receiver.CloseAsync(cancellationToken);

        return messages;
    }

    public async Task DeleteAllActiveMessagesAsync(string queue, CancellationToken cancellationToken)
    {
        var timestamp = timeProvider.GetUtcNow();

        var adminClient = adminClientFactory.CreateClient(context.CurrentClient);
        var azureQueueResponse = await adminClient.GetQueueAsync(queue, cancellationToken);
        var azureQueue = azureQueueResponse.Value;
        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        if (azureQueueRuntimeProperties.ActiveMessageCount < 1)
        {
            return;
        }

        var client = clientFactory.CreateClient(context.CurrentClient);
        if (azureQueue.RequiresSession)
        {
            var receiver = await client.AcceptNextSessionAsync(queue, ServiceBusConstants.PeekLockSessionOptions, cancellationToken: cancellationToken);
            await receiver.CompleteMessagesAsync(timestamp, cancellationToken);
            await receiver.CloseAsync(cancellationToken);
        }
        else
        {
            var receiver = client.CreateReceiver(queue, ServiceBusConstants.PeekLockOptions);
            await receiver.CompleteMessagesAsync(timestamp, cancellationToken);
            await receiver.CloseAsync(cancellationToken);
        }
    }

    public async Task<bool> DeleteActiveMessageAsync(string queue, ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        var adminClient = adminClientFactory.CreateClient(context.CurrentClient);
        var azureQueueResponse = await adminClient.GetQueueAsync(queue, cancellationToken);
        var azureQueue = azureQueueResponse.Value;
        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        if (azureQueueRuntimeProperties.ActiveMessageCount < 1)
        {
            return false;
        }

        var result = false;
        var client = clientFactory.CreateClient(context.CurrentClient);
        if (azureQueue.RequiresSession)
        {
            var receiver = await client.AcceptNextSessionAsync(queue, ServiceBusConstants.PeekLockSessionOptions, cancellationToken: cancellationToken);
            var queueMessage = await receiver.ReceiveMessageAsync(cancellationToken: cancellationToken);
            if (message.Compare(queueMessage))
            {
                await receiver.CompleteMessageAsync(queueMessage, cancellationToken: cancellationToken);
                result = true;
            }
            await receiver.CloseAsync(cancellationToken);
        }
        else
        {
            var receiver = client.CreateReceiver(queue, ServiceBusConstants.PeekLockOptions);
            var queueMessage = await receiver.ReceiveMessageAsync(cancellationToken: cancellationToken);
            if (message.Compare(queueMessage))
            {
                await receiver.CompleteMessageAsync(queueMessage, cancellationToken: cancellationToken);
                result = true;
            }
            await receiver.CloseAsync(cancellationToken);
        }
        return result;
    }

    public async Task<bool> DeadLetterActiveMessagesAsync(string queue, ServiceBusReceivedMessage message, string source, string reason, string description,
        CancellationToken cancellationToken)
    {
        var adminClient = adminClientFactory.CreateClient(context.CurrentClient);
        var azureQueueResponse = await adminClient.GetQueueAsync(queue, cancellationToken);
        var azureQueue = azureQueueResponse.Value;

        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        if (azureQueueRuntimeProperties.ActiveMessageCount < 1)
        {
            return false;
        }

        var result = false;
        var dlqProperties = ServiceBusHelper.GetDeadLetterProperties(source, reason, description);

        var client = clientFactory.CreateClient(context.CurrentClient);
        if (azureQueue.RequiresSession)
        {
            var receiver = await client.AcceptNextSessionAsync(queue, ServiceBusConstants.PeekLockSessionOptions, cancellationToken: cancellationToken);
            var queueMessage = await receiver.ReceiveMessageAsync(cancellationToken: cancellationToken);
            if (message.Compare(queueMessage))
            {
                await receiver.DeadLetterMessageAsync(queueMessage, dlqProperties, cancellationToken: cancellationToken);
                result = true;
            }
            await receiver.CloseAsync(cancellationToken);
        }
        else
        {
            var receiver = client.CreateReceiver(queue, ServiceBusConstants.PeekLockOptions);
            var queueMessage = await receiver.ReceiveMessageAsync(cancellationToken: cancellationToken);
            if (message.Compare(queueMessage))
            {
                await receiver.DeadLetterMessageAsync(queueMessage, dlqProperties, cancellationToken: cancellationToken);
                result = true;
            }

            await receiver.CloseAsync(cancellationToken);
        }

        return result;
    }

    #endregion [Active Queue]

    #region [Dead Letter Queue]

    public async Task<IEnumerable<ServiceBusReceivedMessage>> PeekDeadLetterMessagesAsync(string queue,
        long fromSequenceNumber = RepositoryConstants.DefaultStartIndex,
        int numberOfMessages = RepositoryConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default)
    {
        var adminClient = adminClientFactory.CreateClient(context.CurrentClient);
        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        if (fromSequenceNumber > azureQueueRuntimeProperties.DeadLetterMessageCount)
        {
            return [];
        }

        var deadLetterQueueName = ServiceBusHelper.FormatDeadLetterPath(queue);

        var client = clientFactory.CreateClient(context.CurrentClient);
        var receiver = client.CreateReceiver(deadLetterQueueName, ServiceBusConstants.PeekLockOptions);
        var messages = await receiver.PeekMessagesAsync(numberOfMessages, fromSequenceNumber, cancellationToken);
        await receiver.CloseAsync(cancellationToken);

        return messages;
    }

    public async Task ResubmitAllDeadLetterMessagesAsync(string queue, ResubmitOptions options, CancellationToken cancellationToken)
    {
        var timestamp = timeProvider.GetUtcNow();

        var adminClient = adminClientFactory.CreateClient(context.CurrentClient);
        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        var numberOfMessages = azureQueueRuntimeProperties.DeadLetterMessageCount;
        if (numberOfMessages < 1)
        {
            return;
        }

        var deadLetterQueueName = ServiceBusHelper.FormatDeadLetterPath(queue);

        var client = clientFactory.CreateClient(context.CurrentClient);
        var receiver = client.CreateReceiver(deadLetterQueueName, ServiceBusConstants.PeekLockOptions);
        var sender = client.CreateSender(queue);
        if (options.DeleteMessage)
        {
            await receiver.ResubmitNumberOfMessagesWithDeleteAsync(sender, timestamp, options.GenerateMessageIds, cancellationToken);
        }
        else
        {
            await receiver.ResubmitNumberOfMessagesAsync(sender, timestamp, options.GenerateMessageIds, cancellationToken);
        }

        await receiver.CloseAsync(cancellationToken);
        await sender.CloseAsync(cancellationToken);
    }

    public async Task DeleteAllDeadLetterMessagesAsync(string queue, CancellationToken cancellationToken)
    {
        var timestamp = timeProvider.GetUtcNow();

        var adminClient = adminClientFactory.CreateClient(context.CurrentClient);
        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        var numberOfMessages = azureQueueRuntimeProperties.DeadLetterMessageCount;
        if (numberOfMessages < 1)
        {
            return;
        }

        var deadLetterQueueName = ServiceBusHelper.FormatDeadLetterPath(queue);

        var client = clientFactory.CreateClient(context.CurrentClient);
        var receiver = client.CreateReceiver(deadLetterQueueName, ServiceBusConstants.PeekLockOptions);
        await receiver.CompleteMessagesAsync(timestamp, cancellationToken);
        await receiver.CloseAsync(cancellationToken);
    }

    public async Task<bool> ResubmitDeadLetterMessageAsync(string queue, ServiceBusReceivedMessage receivedMessage, ServiceBusMessage sendMessage, ResubmitOptions options, CancellationToken cancellationToken)
    {
        var adminClient = adminClientFactory.CreateClient(context.CurrentClient);
        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        var numberOfMessages = azureQueueRuntimeProperties.DeadLetterMessageCount;
        if (numberOfMessages < 1)
        {
            return false;
        }

        var deadLetterQueueName = ServiceBusHelper.FormatDeadLetterPath(queue);

        var client = clientFactory.CreateClient(context.CurrentClient);
        var receiver = client.CreateReceiver(deadLetterQueueName, ServiceBusConstants.PeekLockOptions);
        var sender = client.CreateSender(queue);

        var result = await receiver.SearchAndResubmitAsync(sender, receivedMessage, sendMessage, numberOfMessages, options, cancellationToken);

        await receiver.CloseAsync(cancellationToken);
        await sender.CloseAsync(cancellationToken);

        return result;
    }

    public async Task<bool> DeleteDeadLetterMessageAsync(string queue,
        ServiceBusReceivedMessage message,
        CancellationToken cancellationToken)
    {
        var adminClient = adminClientFactory.CreateClient(context.CurrentClient);
        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        var numberOfMessages = azureQueueRuntimeProperties.DeadLetterMessageCount;
        var numberOfMessagesToReceive = Math.Min(numberOfMessages, message.SequenceNumber);
        if (numberOfMessagesToReceive < 1)
        {
            return false;
        }

        var deadLetterQueueName = ServiceBusHelper.FormatDeadLetterPath(queue);

        var client = clientFactory.CreateClient(context.CurrentClient);
        var receiver = client.CreateReceiver(deadLetterQueueName, ServiceBusConstants.PeekLockOptions);
        var result = await receiver.SearchAndCompleteAsync(message, numberOfMessagesToReceive, cancellationToken);

        await receiver.CloseAsync(cancellationToken);

        return result;
    }

    #endregion [Dead Letter Queue]
}
