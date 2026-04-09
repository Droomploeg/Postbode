using Aspire.Hosting;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Droomploeg.DreamOps.Aspire.Tests.Infrastructure;

/// <summary>
/// Shared test fixture that starts the Aspire distributed application once for all tests.
/// Implemented as a thread-safe singleton so test classes can run in parallel
/// while sharing the same Service Bus emulator instance.
/// </summary>
public class DreamOpsFixture
{
    public const string EmulatorConnectionName = "emulator";

    private static readonly SemaphoreSlim InitLock = new(1, 1);
    private static DreamOpsFixture? _instance;
    private static DistributedApplication? _app;

    public string ConnectionString { get; private set; } = null!;
    public ServiceBusClient ServiceBusClient { get; private set; } = null!;

    public static async Task<DreamOpsFixture> GetInstanceAsync()
    {
        if (_instance is not null) return _instance;

        await InitLock.WaitAsync();
        try
        {
            if (_instance is not null) return _instance;

            var fixture = new DreamOpsFixture();
            await fixture.InitializeAsync();
            _instance = fixture;
            return _instance;
        }
        finally
        {
            InitLock.Release();
        }
    }

    private async Task InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Droomploeg_DreamOps_Aspire_Tests_AppHost>();

        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Information);
            logging.AddFilter("Aspire.", LogLevel.Warning);
        });

        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        await _app.ResourceNotifications
            .WaitForResourceHealthyAsync("servicebus")
            .WaitAsync(TimeSpan.FromSeconds(120));

        ConnectionString = (await _app.GetConnectionStringAsync("servicebus"))!;

        var clientOptions = new ServiceBusClientOptions
        {
            RetryOptions = new ServiceBusRetryOptions
            {
                MaxRetries = 10,
                Delay = TimeSpan.FromSeconds(3),
                MaxDelay = TimeSpan.FromSeconds(30),
                Mode = ServiceBusRetryMode.Exponential
            }
        };
        ServiceBusClient = new ServiceBusClient(ConnectionString, clientOptions);

        await WaitForServiceBusReadyAsync();
    }

    // ===== Queue helpers =====

    public async Task PrepareQueueAsync(string queueName)
    {
        await ClearPathAsync(queueName);
        await ClearPathAsync($"{queueName}/$DeadLetterQueue");
    }

    public async Task PrepareSessionQueueAsync(string queueName, string sessionId)
    {
        await ClearSessionPathAsync(queueName, sessionId);
    }

    public async Task SendMessageAsync(string queueName, string body, string? messageId = null, string? sessionId = null)
    {
        var message = new ServiceBusMessage(body);
        if (messageId is not null) message.MessageId = messageId;
        if (sessionId is not null) message.SessionId = sessionId;

        var sender = ServiceBusClient.CreateSender(queueName);
        await sender.SendMessageAsync(message);
        await sender.CloseAsync();
        await Task.Delay(250);
    }

    public async Task SendAndDeadLetterMessageAsync(string queueName, string body, string reason = "Test")
    {
        await SendMessageAsync(queueName, body);

        var receiver = ServiceBusClient.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        });

        var message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(10));
        if (message is not null)
        {
            await receiver.DeadLetterMessageAsync(message, reason, "Test dead-letter");
        }
        await receiver.CloseAsync();
        await Task.Delay(250);
    }

    public async Task<IReadOnlyList<ServiceBusReceivedMessage>> PeekActiveMessagesAsync(string queueName, int maxMessages = 10)
    {
        var receiver = ServiceBusClient.CreateReceiver(queueName);
        var messages = await receiver.PeekMessagesAsync(maxMessages);
        await receiver.CloseAsync();
        return messages;
    }

    public async Task<IReadOnlyList<ServiceBusReceivedMessage>> PeekDeadLetterMessagesAsync(string queueName, int maxMessages = 10)
    {
        var dlqPath = $"{queueName}/$DeadLetterQueue";
        var receiver = ServiceBusClient.CreateReceiver(dlqPath);
        var messages = await receiver.PeekMessagesAsync(maxMessages);
        await receiver.CloseAsync();
        return messages;
    }

    public async Task<int> CountActiveMessagesAsync(string queueName) =>
        (await PeekActiveMessagesAsync(queueName, 1000)).Count;

    public async Task<int> CountDeadLetterMessagesAsync(string queueName) =>
        (await PeekDeadLetterMessagesAsync(queueName, 1000)).Count;

    // ===== Topic/Subscription helpers =====

    public async Task PrepareSubscriptionAsync(string topicName, string subscriptionName)
    {
        var subPath = $"{topicName}/subscriptions/{subscriptionName}";
        await ClearPathAsync(subPath);
        await ClearPathAsync($"{subPath}/$DeadLetterQueue");
    }

    public async Task PrepareSessionSubscriptionAsync(string topicName, string subscriptionName, string sessionId)
    {
        var subPath = $"{topicName}/subscriptions/{subscriptionName}";
        await ClearSessionPathAsync(subPath, sessionId);
    }

    public async Task SendTopicMessageAsync(string topicName, string body, string? messageId = null, string? sessionId = null)
    {
        var message = new ServiceBusMessage(body);
        if (messageId is not null) message.MessageId = messageId;
        if (sessionId is not null) message.SessionId = sessionId;

        var sender = ServiceBusClient.CreateSender(topicName);
        await sender.SendMessageAsync(message);
        await sender.CloseAsync();
        await Task.Delay(250);
    }

    public async Task SendAndDeadLetterSubscriptionMessageAsync(string topicName, string subscriptionName, string body, string reason = "Test")
    {
        await SendTopicMessageAsync(topicName, body);

        var subPath = $"{topicName}/subscriptions/{subscriptionName}";
        var receiver = ServiceBusClient.CreateReceiver(subPath, new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        });

        var message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(30));
        if (message is null)
        {
            await receiver.CloseAsync();
            throw new InvalidOperationException($"No message received on {subPath} within timeout");
        }

        await receiver.DeadLetterMessageAsync(message, reason, "Test dead-letter");
        await receiver.CloseAsync();
        await Task.Delay(250);
    }

    public async Task<IReadOnlyList<ServiceBusReceivedMessage>> PeekSubscriptionMessagesAsync(string topicName, string subscriptionName, int maxMessages = 10)
    {
        var subPath = $"{topicName}/subscriptions/{subscriptionName}";
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);

        while (DateTime.UtcNow < deadline)
        {
            var receiver = ServiceBusClient.CreateReceiver(subPath);
            var messages = await receiver.PeekMessagesAsync(maxMessages);
            await receiver.CloseAsync();

            if (messages.Count > 0)
            {
                return messages;
            }

            await Task.Delay(100);
        }

        return [];
    }

    public async Task<IReadOnlyList<ServiceBusReceivedMessage>> PeekSubscriptionDeadLetterMessagesAsync(string topicName, string subscriptionName, int maxMessages = 10)
    {
        var dlqPath = $"{topicName}/subscriptions/{subscriptionName}/$DeadLetterQueue";
        var receiver = ServiceBusClient.CreateReceiver(dlqPath);
        var messages = await receiver.PeekMessagesAsync(maxMessages);
        await receiver.CloseAsync();
        return messages;
    }

    public async Task<int> CountSubscriptionActiveMessagesAsync(string topicName, string subscriptionName) =>
        (await PeekSubscriptionMessagesAsync(topicName, subscriptionName, 1000)).Count;

    public async Task<int> CountSubscriptionDeadLetterMessagesAsync(string topicName, string subscriptionName) =>
        (await PeekSubscriptionDeadLetterMessagesAsync(topicName, subscriptionName, 1000)).Count;

    // ===== Private =====

    private async Task ClearSessionPathAsync(string path, string sessionId)
    {
        try
        {
            var receiver = await ServiceBusClient.AcceptSessionAsync(path, sessionId, new ServiceBusSessionReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
            });

            while (true)
            {
                var messages = await receiver.ReceiveMessagesAsync(100, TimeSpan.FromSeconds(2));
                if (messages.Count == 0) break;
            }

            await receiver.CloseAsync();
        }
        catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.ServiceTimeout)
        {
            // No session available — queue is already empty
        }
    }

    private async Task ClearPathAsync(string path)
    {
        var receiver = ServiceBusClient.CreateReceiver(path, new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        while (true)
        {
            var messages = await receiver.ReceiveMessagesAsync(100, TimeSpan.FromSeconds(2));
            if (messages.Count == 0) break;
        }

        await receiver.CloseAsync();
    }

    private async Task WaitForServiceBusReadyAsync()
    {
        Exception? lastException = null;
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(120);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var sender = ServiceBusClient.CreateSender("test-queue");
                await sender.SendMessageAsync(new ServiceBusMessage("healthcheck"));
                await sender.CloseAsync();

                var receiver = ServiceBusClient.CreateReceiver("test-queue", new ServiceBusReceiverOptions
                {
                    ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
                });
                await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));
                await receiver.CloseAsync();
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
        throw new TimeoutException(
            $"Service Bus emulator did not become ready in time. Last error: {lastException?.Message}",
            lastException);
    }
}
