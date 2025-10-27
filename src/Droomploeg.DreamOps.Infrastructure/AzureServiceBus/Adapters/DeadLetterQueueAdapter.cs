using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Application;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Extensions;
using Droomploeg.DreamOps.Infrastructure.Contexts;
using Microsoft.Extensions.Azure;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Adapters;

public class DeadLetterQueueAdapter : IDeadLetterQueueAdapter<ServiceBusMessage, ServiceBusReceivedMessage>
{
    private readonly ApplicationContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IAzureClientFactory<ServiceBusAdministrationClient> _adminClientFactory;
    private readonly IAzureClientFactory<ServiceBusClient> _clientFactory;

    public DeadLetterQueueAdapter(
        TimeProvider timeProvider,
        ApplicationContext context,
        IAzureClientFactory<ServiceBusAdministrationClient> adminClientFactory,
        IAzureClientFactory<ServiceBusClient> clientFactory)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _adminClientFactory = adminClientFactory ?? throw new ArgumentNullException(nameof(adminClientFactory));
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <see cref="IDeadLetterQueueRepository{TSendMessage, TReceiveMessage}"/>
    public async Task<ICollection<ServiceBusReceivedMessage>> PeekMessagesAsync(string queue,
        long fromSequenceNumber = EntityAdapterConstants.DefaultStartIndex,
        int numberOfMessages = EntityAdapterConstants.DefaultNumberOfMessage,
        CancellationToken cancellationToken = default)
    {
        var adminClient = _adminClientFactory.CreateClient(_context);
        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        if (fromSequenceNumber > azureQueueRuntimeProperties.DeadLetterMessageCount)
        {
            return [];
        }

        var deadLetterQueueName = ServiceBusHelper.FormatDeadLetterPath(queue);

        var client = _clientFactory.CreateClient(_context);
        var receiver = client.CreateReceiver(deadLetterQueueName, ServiceBusConstants.PeekLockOptions);
        var messages = await receiver.PeekMessagesAsync(numberOfMessages, fromSequenceNumber, cancellationToken);
        await receiver.CloseAsync(cancellationToken);

        return [.. messages];
    }

    public async Task ResubmitAllMessagesAsync(string queue, ResubmitOptions options, CancellationToken cancellationToken)
    {
        var timestamp = _timeProvider.GetUtcNow();

        var adminClient = _adminClientFactory.CreateClient(_context);
        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        var numberOfMessages = azureQueueRuntimeProperties.DeadLetterMessageCount;
        if (numberOfMessages < 1)
        {
            return;
        }

        var deadLetterQueueName = ServiceBusHelper.FormatDeadLetterPath(queue);

        var client = _clientFactory.CreateClient(_context);
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
        var timestamp = _timeProvider.GetUtcNow();

        var adminClient = _adminClientFactory.CreateClient(_context);
        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        var numberOfMessages = azureQueueRuntimeProperties.DeadLetterMessageCount;
        if (numberOfMessages < 1)
        {
            return;
        }

        var deadLetterQueueName = ServiceBusHelper.FormatDeadLetterPath(queue);

        var client = _clientFactory.CreateClient(_context);
        var receiver = client.CreateReceiver(deadLetterQueueName, ServiceBusConstants.PeekLockOptions);
        await receiver.CompleteMessagesAsync(timestamp, cancellationToken);
        await receiver.CloseAsync(cancellationToken);
    }

    public async Task<bool> ResubmitMessageAsync(string queue, ServiceBusReceivedMessage receivedMessage, ServiceBusMessage sendMessage, ResubmitOptions options, CancellationToken cancellationToken)
    {
        var adminClient = _adminClientFactory.CreateClient(_context);
        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        var numberOfMessages = azureQueueRuntimeProperties.DeadLetterMessageCount;
        if (numberOfMessages < 1)
        {
            return false;
        }

        var deadLetterQueueName = ServiceBusHelper.FormatDeadLetterPath(queue);

        var client = _clientFactory.CreateClient(_context);
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
        var adminClient = _adminClientFactory.CreateClient(_context);
        var azureQueueRuntimePropertiesResponse = await adminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
        var azureQueueRuntimeProperties = azureQueueRuntimePropertiesResponse.Value;
        var numberOfMessages = azureQueueRuntimeProperties.DeadLetterMessageCount;
        var numberOfMessagesToReceive = Math.Min(numberOfMessages, message.SequenceNumber);
        if (numberOfMessagesToReceive < 1)
        {
            return false;
        }

        var deadLetterQueueName = ServiceBusHelper.FormatDeadLetterPath(queue);

        var client = _clientFactory.CreateClient(_context);
        var receiver = client.CreateReceiver(deadLetterQueueName, ServiceBusConstants.PeekLockOptions);
        var result = await receiver.SearchAndCompleteAsync(message, numberOfMessagesToReceive, cancellationToken);

        await receiver.CloseAsync(cancellationToken);

        return result;
    }
}
