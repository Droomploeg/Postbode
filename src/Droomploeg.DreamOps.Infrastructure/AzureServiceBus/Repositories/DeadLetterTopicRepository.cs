using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Core;
using Droomploeg.DreamOps.Core.Models;
using Droomploeg.DreamOps.Core.Repositories;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Extensions;
using Microsoft.Extensions.Azure;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Repositories
{
    public class DeadLetterTopicRepository(
        TimeProvider timeProvider,
        IServiceBusConnectionAccessor connectionAccessor,
        IAzureClientFactory<ServiceBusAdministrationClient> adminClientFactory,
        IAzureClientFactory<ServiceBusClient> clientFactory) : IDeadLetterTopicRepository<ServiceBusMessage, ServiceBusReceivedMessage>
    {
        public async Task<IEnumerable<ServiceBusReceivedMessage>> PeekMessagesAsync(string topic, string subscription,
            long fromSequenceNumber = RepositoryConstants.DefaultStartIndex,
            int numberOfMessages = RepositoryConstants.DefaultNumberOfMessage,
            CancellationToken cancellationToken = default)
        {
            var connection = await connectionAccessor.GetCurrentAsync();
            var adminClient = adminClientFactory.CreateClient(connection.Name);
            var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
            var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
            if (fromSequenceNumber > azureSubscriptionRuntimeProperties.DeadLetterMessageCount)
            {
                return [];
            }

            var deadLetterSubscription = ServiceBusHelper.FormatDeadLetterPath(topic, subscription);

            var client = clientFactory.CreateClient(connection.Name);
            var receiver = client.CreateReceiver(deadLetterSubscription, ServiceBusConstants.PeekLockOptions);
            var messages = await receiver.PeekMessagesAsync(numberOfMessages, fromSequenceNumber, cancellationToken);
            await receiver.CloseAsync(cancellationToken);
            return messages;
        }

        public async Task ResubmitAllMessagesAsync(string topic, string subscription, ResubmitOptions options, CancellationToken cancellationToken)
        {
            var connection = await connectionAccessor.GetCurrentAsync();
            var timestamp = timeProvider.GetUtcNow();


            var adminClient = adminClientFactory.CreateClient(connection.Name);
            var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
            var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
            var numberOfMessages = azureSubscriptionRuntimeProperties.DeadLetterMessageCount;
            if (numberOfMessages < 1)
            {
                return;
            }

            var deadLetterSubscription = ServiceBusHelper.FormatDeadLetterPath(topic, subscription);

            var client = clientFactory.CreateClient(connection.Name);
            var receiver = client.CreateReceiver(deadLetterSubscription, ServiceBusConstants.PeekLockOptions);
            var sender = client.CreateSender(topic);
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

        public async Task DeleteAllMessagesAsync(string topic, string subscription, CancellationToken cancellationToken)
        {
            var connection = await connectionAccessor.GetCurrentAsync();
            var timestamp = timeProvider.GetUtcNow();

            var adminClient = adminClientFactory.CreateClient(connection.Name);
            var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
            var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
            var numberOfMessages = azureSubscriptionRuntimeProperties.DeadLetterMessageCount;
            if (numberOfMessages < 1)
            {
                return;
            }

            var deadLetterSubscription = ServiceBusHelper.FormatDeadLetterPath(topic, subscription);

            var client = clientFactory.CreateClient(connection.Name);
            var receiver = client.CreateReceiver(deadLetterSubscription, ServiceBusConstants.PeekLockOptions);
            await receiver.CompleteMessagesAsync(timestamp, cancellationToken);
            await receiver.CloseAsync(cancellationToken);
        }

        public async Task<bool> ResubmitMessageAsync(string topic, string subscription, ServiceBusReceivedMessage receivedMessage, ServiceBusMessage sendMessage, ResubmitOptions options, CancellationToken cancellationToken)
        {
            var connection = await connectionAccessor.GetCurrentAsync();
            var adminClient = adminClientFactory.CreateClient(connection.Name);
            var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
            var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
            var numberOfMessages = azureSubscriptionRuntimeProperties.DeadLetterMessageCount;
            if (numberOfMessages < 1)
            {
                return false;
            }

            var deadLetterSubscription = ServiceBusHelper.FormatDeadLetterPath(topic, subscription);

            var client = clientFactory.CreateClient(connection.Name);
            var receiver = client.CreateReceiver(deadLetterSubscription, ServiceBusConstants.PeekLockOptions);
            var sender = client.CreateSender(topic);

            var result = await receiver.SearchAndResubmitAsync(sender, receivedMessage, sendMessage, numberOfMessages, options, cancellationToken);
            await receiver.CloseAsync(cancellationToken);
            await sender.CloseAsync(cancellationToken);
            return result;
        }

        public async Task<bool> DeleteMessageAsync(string topic, string subscription, ServiceBusReceivedMessage message, CancellationToken cancellationToken)
        {
            var connection = await connectionAccessor.GetCurrentAsync();
            var adminClient = adminClientFactory.CreateClient(connection.Name);
            var azureSubscriptionRuntimePropertiesResponse = await adminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
            var azureSubscriptionRuntimeProperties = azureSubscriptionRuntimePropertiesResponse.Value;
            var numberOfMessages = azureSubscriptionRuntimeProperties.DeadLetterMessageCount;
            var numberOfMessagesToReceive = Math.Min(numberOfMessages, message.SequenceNumber);
            if (numberOfMessagesToReceive < 1)
            {
                return false;
            }

            var deadLetterSubscription = ServiceBusHelper.FormatDeadLetterPath(topic, subscription);
            var client = clientFactory.CreateClient(connection.Name);
            var receiver = client.CreateReceiver(deadLetterSubscription, ServiceBusConstants.PeekLockOptions);
            var result = await receiver.SearchAndCompleteAsync(message, numberOfMessagesToReceive, cancellationToken);
            await receiver.CloseAsync(cancellationToken);
            return result;
        }
    }
}
