using Azure.Messaging.ServiceBus;
using Droomploeg.Postbode.Application.ServiceBus.Adapters;
using Droomploeg.Postbode.Application.ServiceBus.Factories;
using Droomploeg.Postbode.Application.ServiceBus.Services;
using Droomploeg.Postbode.Application.Workers.Services;
using Droomploeg.Postbode.Domain.ServiceBus.Types;
using Droomploeg.Postbode.Infrastructure.Audit;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Adapters;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Factories;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Services;
using Droomploeg.Postbode.Infrastructure.Contexts;
using Droomploeg.Postbode.Infrastructure.Workers.Services;
using Droomploeg.Postbode.Application.Workers.Dispatcher;
using Droomploeg.Postbode.WebApp.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Droomploeg.Postbode.Aspire.Tests.Infrastructure;

/// <summary>
/// Registers the full application service stack for testing against the Service Bus emulator.
/// Replaces Blazor-specific services (WebContextSetter, security) with test implementations.
/// </summary>
internal static class TestServiceCollectionExtensions
{
    internal static IServiceCollection AddTestApplicationServices(
        this IServiceCollection services,
        string connectionString,
        string connectionName)
    {
        // Context — test implementation instead of WebContextSetter
        var testContext = TestContextSetter.CreateTestContext(connectionName);
        services.AddScoped(_ => testContext);
        services.AddTransient<IContextSetter>(_ => new TestContextSetter(testContext));

        // Azure Service Bus clients: the user client and the service account admin client both point to the emulator
        services.AddScoped<IUserServiceBusClientProvider>(_ => new TestUserServiceBusClientProvider(connectionString));
        services.AddSingleton<IServiceAccountAdminClientProvider>(new TestServiceAccountAdminClientProvider(connectionString));

        // Factories
        services
            .AddTransient<IAdapterFactory<IActiveQueueAdapter<ServiceBusMessage, ServiceBusReceivedMessage>>,
                AdapterFactory<IActiveQueueAdapter<ServiceBusMessage, ServiceBusReceivedMessage>>>()
            .AddTransient<IAdapterFactory<IDeadLetterQueueAdapter<ServiceBusMessage, ServiceBusReceivedMessage>>,
                AdapterFactory<IDeadLetterQueueAdapter<ServiceBusMessage, ServiceBusReceivedMessage>>>()
            .AddTransient<IAdapterFactory<IActiveTopicAdapter<ServiceBusMessage, ServiceBusReceivedMessage>>,
                AdapterFactory<IActiveTopicAdapter<ServiceBusMessage, ServiceBusReceivedMessage>>>()
            .AddTransient<IAdapterFactory<IDeadLetterTopicAdapter<ServiceBusMessage, ServiceBusReceivedMessage>>,
                AdapterFactory<IDeadLetterTopicAdapter<ServiceBusMessage, ServiceBusReceivedMessage>>>();

        // Adapters — real adapters with test session info provider
        services
            .AddTransient<IActiveQueueAdapter<ServiceBusMessage, ServiceBusReceivedMessage>, ActiveQueueAdapter>()
            .AddTransient<IDeadLetterQueueAdapter<ServiceBusMessage, ServiceBusReceivedMessage>, DeadLetterQueueAdapter>()
            .AddTransient<IActiveTopicAdapter<ServiceBusMessage, ServiceBusReceivedMessage>, ActiveTopicAdapter>()
            .AddTransient<IDeadLetterTopicAdapter<ServiceBusMessage, ServiceBusReceivedMessage>, DeadLetterTopicAdapter>()
            .AddTransient<IRuntimeInfoAdapter, RuntimeInfoAdapter>();

        // Services
        services
            .AddSingleton(TimeProvider.System)
            .AddSingleton<IWorkerService, WorkerService>()
            .AddSingleton<TestWorkerDispatcher>()
            .AddSingleton<IWorkerDispatcher>(sp => sp.GetRequiredService<TestWorkerDispatcher>())
            .AddSingleton<TestAuditLogger>()
            .AddSingleton<IAuditLogger>(sp => sp.GetRequiredService<TestAuditLogger>())
            .AddTransient<INotificationService, NotificationService>()
            .AddTransient<IQueueService<ServiceBusMessage, ServiceBusReceivedMessage>,
                QueueService<ServiceBusMessage, ServiceBusReceivedMessage>>()
            .AddTransient<ITopicService<ServiceBusMessage, ServiceBusReceivedMessage>,
                TopicService<ServiceBusMessage, ServiceBusReceivedMessage>>()
            .AddTransient<IRuntimeInfoService, RuntimeInfoService>();

        // Configuration + ApplicationInsightsLink (required by ReceivedMessageControl)
        var configuration = new ConfigurationBuilder().Build();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddTransient<ApplicationInsightsLink>();

        // Logging
        services.AddLogging(logging => logging.AddConsole());

        return services;
    }
}
