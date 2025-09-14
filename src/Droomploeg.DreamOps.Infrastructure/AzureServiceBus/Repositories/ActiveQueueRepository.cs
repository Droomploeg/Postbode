using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Core;
using Droomploeg.DreamOps.Core.Repositories;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Extensions;
using Microsoft.Extensions.Azure;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Repositories;

public class ActiveQueueRepository(
    TimeProvider timeProvider,
    IServiceBusConnectionAccessor connectionAccessor,
    IAzureClientFactory<ServiceBusAdministrationClient> adminClientFactory,
    IAzureClientFactory<ServiceBusClient> clientFactory) : IActiveQueueRepository<ServiceBusMessage, ServiceBusReceivedMessage>
{
    public async Task SendAsync(string queue, ICollection<ServiceBusMessage> messages, CancellationToken cancellationToken = default)
    {
        var connection = await connectionAccessor.GetCurrentAsync();
        var client = clientFactory.CreateClient(connection.Name);
        var sender = client
            .CreateSender(queue);

        await sender.SendBulkMessageAsync(messages, cancellationToken);
        await sender.CloseAsync(cancellationToken);
    }

    public async Task<IEnumerable<ServiceBusReceivedMessage>> PeekMessagesAsync(string queue,
        long fromSequenceNumber = RepositoryConstants.DefaultStartIndex,
        int numberOfMessages = RepositoryConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default)
    {
        var connection = await connectionAccessor.GetCurrentAsync();

        var adminClient = adminClientFactory.CreateClient(connection.Name);
        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        if (fromSequenceNumber > azureQueueRuntimeProperties.ActiveMessageCount)
        {
            return [];
        }

        var client = clientFactory.CreateClient(connection.Name);
        var receiver = client.CreateReceiver(queue, ServiceBusConstants.PeekLockOptions);
        var messages = await receiver.PeekMessagesAsync(numberOfMessages, fromSequenceNumber, cancellationToken);
        await receiver.CloseAsync(cancellationToken);

        return messages;
    }

    public async Task DeleteAllMessagesAsync(string queue, CancellationToken cancellationToken)
    {
        var connection = await connectionAccessor.GetCurrentAsync();

        var timestamp = timeProvider.GetUtcNow();

        var adminClient = adminClientFactory.CreateClient(connection.Name);
        var azureQueueResponse = await adminClient.GetQueueAsync(queue, cancellationToken);
        var azureQueue = azureQueueResponse.Value;
        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        if (azureQueueRuntimeProperties.ActiveMessageCount < 1)
        {
            return;
        }

        var client = clientFactory.CreateClient(connection.Name);
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

    public async Task<bool> DeleteMessageAsync(string queue, ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        var connection = await connectionAccessor.GetCurrentAsync();

        var adminClient = adminClientFactory.CreateClient(connection.Name);
        var azureQueueResponse = await adminClient.GetQueueAsync(queue, cancellationToken);
        var azureQueue = azureQueueResponse.Value;
        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        if (azureQueueRuntimeProperties.ActiveMessageCount < 1)
        {
            return false;
        }

        var result = false;
        var client = clientFactory.CreateClient(connection.Name);
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

    public async Task<bool> DeadLetterMessagesAsync(string queue, ServiceBusReceivedMessage message, string source, string reason, string description,
        CancellationToken cancellationToken)
    {
        var connection = await connectionAccessor.GetCurrentAsync();

        var adminClient = adminClientFactory.CreateClient(connection.Name);
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

        var client = clientFactory.CreateClient(connection.Name);
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
}
