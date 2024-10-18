using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus;
using Droomploeg.DreamOps.WebApp.Configurations.Options;
using FluentAssertions;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;

namespace Droomploeg.DreamOps.IntegrationsTests.Common.ServiceBus;

public class QueueTestContext
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusAdministrationClient _adminClient;

    public QueueTestContext(
        IAzureClientFactory<ServiceBusClient> clientFactory,
        IAzureClientFactory<ServiceBusAdministrationClient> adminClientFactory,
        IConfiguration configuration)
    {
        var connectionList = configuration.GetSection(AzureServiceBusConnection.SectionName).Get<List<AzureServiceBusConnection>>() ?? [];
        var clientName = connectionList[0].Name;

        _client = clientFactory.CreateClient(clientName);
        _adminClient = adminClientFactory.CreateClient(clientName);
    }

    internal async Task ClearAll(string queueName)
    {
        await ClearAsync(queueName);
        await ClearDeadLettersAsync(queueName);
    }

    internal async Task ClearAsync(string queueName)
    {
        var runtimeInfo = await _adminClient.GetQueueRuntimePropertiesAsync(queueName);
        if (runtimeInfo.Value.ActiveMessageCount == 0)
        {
            return;
        }

        var isCleared = await ClearUntilEmptyAsync(queueName);
        if (!isCleared)
        {
            throw new TimeoutException("Timeout while clearing queue");
        }
    }

    internal async Task ClearDeadLettersAsync(string queueName)
    {
        var dlqName = ServiceBusHelper.FormatDeadLetterPath(queueName);
        var runtimeInfo = await _adminClient.GetQueueRuntimePropertiesAsync(queueName);
        if (runtimeInfo.Value.DeadLetterMessageCount == 0)
        {
            return;
        }

        var isCleared = await ClearUntilEmptyAsync(dlqName);
        if (!isCleared)
        {
            throw new TimeoutException("Timeout while clearing dead-letter queue");
        }
    }

    internal async Task SendAsync(string queueName, ServiceBusMessage message)
    {
        var runtimeInfo = await _adminClient.GetQueueRuntimePropertiesAsync(queueName);
        var currentMessageCount = runtimeInfo.Value.ActiveMessageCount;

        var sender = _client.CreateSender(queueName);
        try
        {
            await sender.SendMessageAsync(message);
            Func<Task<bool>> polling = async () =>
            {
                runtimeInfo = await _adminClient.GetQueueRuntimePropertiesAsync(queueName);
                return runtimeInfo.Value.ActiveMessageCount == currentMessageCount + 1;
            };
            await polling.WaitWithDelay(TimeSpan.FromSeconds(10), $"Timeout while waiting for messages in queue ({currentMessageCount + 1})");
        }
        finally
        {
            sender?.CloseAsync();
        }
    }

    internal async Task<IEnumerable<ServiceBusReceivedMessage>> DeadLetterAsync(string queueName, string reason, int maxCount = 100)
    {
        var dlqName = ServiceBusHelper.FormatDeadLetterPath(queueName);
        var runtimeInfo = await _adminClient.GetQueueRuntimePropertiesAsync(queueName);
        var currentDlqMessageCount = runtimeInfo.Value.DeadLetterMessageCount;

        var receiver = _client.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        });

        try
        {
            var messages = await receiver.ReceiveMessagesAsync(maxMessages: maxCount, maxWaitTime: TimeSpan.FromSeconds(5));
            if (messages.Count == 0)
            {
                return [];
            }

            messages
                .ToList()
                .ForEach(async m =>
            {
                var dlqProperties = new Dictionary<string, object>
                {
                    { ServiceBusConstants.DeadLetterSourceKey, "XUnit" },
                    { ServiceBusConstants.DeadLetterReasonKey, reason },
                    { ServiceBusConstants.DeadLetterErrorDescriptionKey, "Test purpose" }
                };
                await receiver.DeadLetterMessageAsync(m, dlqProperties);
            });

            Func<Task<bool>> polling = async () =>
            {
                runtimeInfo = await _adminClient.GetQueueRuntimePropertiesAsync(queueName);
                return runtimeInfo.Value.DeadLetterMessageCount == currentDlqMessageCount + messages.Count;
            };
            await polling.WaitWithDelay(TimeSpan.FromSeconds(10), $"Timeout while waiting for messages in dead-letter queue ({currentDlqMessageCount + messages.Count})");

            return messages;
        }
        finally
        {
            receiver?.CloseAsync();
        }

        //var stopWatch = new Stopwatch();
        //stopWatch.Start();
        //while (true)
        //{
        //    runtimeInfo = await _adminClient.GetQueueRuntimePropertiesAsync(queueName);
        //    if (runtimeInfo.Value.DeadLetterMessageCount == currentDlqMessageCount + messages.Count)
        //    {
        //        return messages;
        //    }

        //    if (stopWatch.Elapsed > TimeSpan.FromSeconds(10))
        //    {
        //        throw new TimeoutException($"Timeout while waiting for messages in dead-letter queue ({currentDlqMessageCount + messages.Count})");
        //    }
        //    await Task.Delay(TimeSpan.FromSeconds(0.5));
        //}
    }

    internal async Task<QueueRuntimeProperties> GetRuntimeInfo(string queueName)
    {
        var runtimeInfo = await _adminClient.GetQueueRuntimePropertiesAsync(queueName);
        return runtimeInfo.Value;
    }

    private async Task<bool> ClearUntilEmptyAsync(string queueName)
    {
        var receiver = _client.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        while (true)
        {
            var messages = await receiver.ReceiveMessagesAsync(maxMessages: 100, maxWaitTime: TimeSpan.FromSeconds(5));
            if (messages.Count == 0)
            {
                await receiver.CloseAsync();
                return true;
            }

            if (stopwatch.Elapsed > TimeSpan.FromSeconds(10))
            {
                await receiver.CloseAsync();
                return false;
            }
        }

    }
}
