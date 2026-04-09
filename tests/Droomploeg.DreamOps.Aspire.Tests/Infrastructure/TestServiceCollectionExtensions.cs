using Azure.Messaging.ServiceBus;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Application.ServiceBus.Factories;
using Droomploeg.DreamOps.Application.ServiceBus.Services;
using Droomploeg.DreamOps.Application.Workers.Services;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Droomploeg.DreamOps.Infrastructure.Audit;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Adapters;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Factories;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Services;
using Droomploeg.DreamOps.Infrastructure.Contexts;
using Droomploeg.DreamOps.Infrastructure.Workers.Services;
using Droomploeg.DreamOps.Application.Workers.Dispatcher;
using Droomploeg.DreamOps.WebApp.Common;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Droomploeg.DreamOps.Aspire.Tests.Infrastructure;

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

        // Azure Service Bus clients — both UserAccount and ServiceAccount point to emulator
        services.AddAzureClients(builder =>
        {
            var userClientName = ServiceBusConnection.ClientName(connectionName, ServiceBusConnectionType.UserAccount);
            builder.AddServiceBusClient(connectionString).WithName(userClientName);

            var serviceClientName = ServiceBusConnection.ClientName(connectionName, ServiceBusConnectionType.ServiceAccount);
            builder.AddServiceBusClient(connectionString).WithName(serviceClientName);

            builder.AddServiceBusAdministrationClient(connectionString).WithName(serviceClientName);
        });

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
