using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Core;
using Droomploeg.DreamOps.Core.Models;
using Droomploeg.DreamOps.Core.Repositories;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Extensions;
using Microsoft.Extensions.Azure;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Repositories;

public class DeadLetterQueueRepository(
    TimeProvider timeProvider,
    IServiceBusConnectionAccessor connectionAccessor,
    IAzureClientFactory<ServiceBusAdministrationClient> adminClientFactory,
    IAzureClientFactory<ServiceBusClient> clientFactory) : IDeadLetterQueueRepository<ServiceBusMessage, ServiceBusReceivedMessage>
{
    /// <see cref="IDeadLetterQueueRepository{TSendMessage, TReceiveMessage}"/>
    public async Task<IEnumerable<ServiceBusReceivedMessage>> PeekMessagesAsync(string queue,
        long fromSequenceNumber = RepositoryConstants.DefaultStartIndex,
        int numberOfMessages = RepositoryConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default)
    {
        var connection = await connectionAccessor.GetCurrentAsync();
        var adminClient = adminClientFactory.CreateClient(connection.Name);
        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        if (fromSequenceNumber > azureQueueRuntimeProperties.DeadLetterMessageCount)
        {
            return [];
        }

        var deadLetterQueueName = ServiceBusHelper.FormatDeadLetterPath(queue);

        var client = clientFactory.CreateClient(connection.Name);
        var receiver = client.CreateReceiver(deadLetterQueueName, ServiceBusConstants.PeekLockOptions);
        var messages = await receiver.PeekMessagesAsync(numberOfMessages, fromSequenceNumber, cancellationToken);
        await receiver.CloseAsync(cancellationToken);

        return messages;
    }

    public async Task ResubmitAllMessagesAsync(string queue, ResubmitOptions options, CancellationToken cancellationToken)
    {
        var timestamp = timeProvider.GetUtcNow();

        var connection = await connectionAccessor.GetCurrentAsync();
        var adminClient = adminClientFactory.CreateClient(connection.Name);
        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        var numberOfMessages = azureQueueRuntimeProperties.DeadLetterMessageCount;
        if (numberOfMessages < 1)
        {
            return;
        }

        var deadLetterQueueName = ServiceBusHelper.FormatDeadLetterPath(queue);

        var client = clientFactory.CreateClient(connection.Name);
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

    public async Task DeleteAllMessagesAsync(string queue, CancellationToken cancellationToken)
    {
        var timestamp = timeProvider.GetUtcNow();

        var connection = await connectionAccessor.GetCurrentAsync();
        var adminClient = adminClientFactory.CreateClient(connection.Name);
        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        var numberOfMessages = azureQueueRuntimeProperties.DeadLetterMessageCount;
        if (numberOfMessages < 1)
        {
            return;
        }

        var deadLetterQueueName = ServiceBusHelper.FormatDeadLetterPath(queue);

        var client = clientFactory.CreateClient(connection.Name);
        var receiver = client.CreateReceiver(deadLetterQueueName, ServiceBusConstants.PeekLockOptions);
        await receiver.CompleteMessagesAsync(timestamp, cancellationToken);
        await receiver.CloseAsync(cancellationToken);
    }

    public async Task<bool> ResubmitMessageAsync(string queue, ServiceBusReceivedMessage receivedMessage, ServiceBusMessage sendMessage, ResubmitOptions options, CancellationToken cancellationToken)
    {
        var connection = await connectionAccessor.GetCurrentAsync();
        var adminClient = adminClientFactory.CreateClient(connection.Name);
        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        var numberOfMessages = azureQueueRuntimeProperties.DeadLetterMessageCount;
        if (numberOfMessages < 1)
        {
            return false;
        }

        var deadLetterQueueName = ServiceBusHelper.FormatDeadLetterPath(queue);

        var client = clientFactory.CreateClient(connection.Name);
        var receiver = client.CreateReceiver(deadLetterQueueName, ServiceBusConstants.PeekLockOptions);
        var sender = client.CreateSender(queue);

        var result = await receiver.SearchAndResubmitAsync(sender, receivedMessage, sendMessage, numberOfMessages, options, cancellationToken);

        await receiver.CloseAsync(cancellationToken);
        await sender.CloseAsync(cancellationToken);

        return result;
    }

    public async Task<bool> DeleteMessageAsync(string queue,
        ServiceBusReceivedMessage message,
        CancellationToken cancellationToken)
    {
        var connection = await connectionAccessor.GetCurrentAsync();
        var adminClient = adminClientFactory.CreateClient(connection.Name);
        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        var numberOfMessages = azureQueueRuntimeProperties.DeadLetterMessageCount;
        var numberOfMessagesToReceive = Math.Min(numberOfMessages, message.SequenceNumber);
        if (numberOfMessagesToReceive < 1)
        {
            return false;
        }

        var deadLetterQueueName = ServiceBusHelper.FormatDeadLetterPath(queue);

        var client = clientFactory.CreateClient(connection.Name);
        var receiver = client.CreateReceiver(deadLetterQueueName, ServiceBusConstants.PeekLockOptions);
        var result = await receiver.SearchAndCompleteAsync(message, numberOfMessagesToReceive, cancellationToken);

        await receiver.CloseAsync(cancellationToken);

        return result;
    }
}
