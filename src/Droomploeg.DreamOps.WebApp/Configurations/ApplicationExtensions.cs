using Azure.Messaging.ServiceBus;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Application.ServiceBus.Factories;
using Droomploeg.DreamOps.Application.ServiceBus.Services;
using Droomploeg.DreamOps.Application.Workers.Services;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Adapters;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Factories;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Services;
using Droomploeg.DreamOps.Infrastructure.Contexts;
using Droomploeg.DreamOps.Infrastructure.Workers.Services;

namespace Droomploeg.DreamOps.WebApp.Configurations;

internal static class ApplicationExtensions
{
    internal static IServiceCollection AddApplicationCore(this IServiceCollection services) => services.AddSingleton(TimeProvider.System)
            .AddContext()
            .AddFactories()
            .AddAdapters()
            .AddServices();

    private static IServiceCollection AddContext(this IServiceCollection services)
    {
        services
            .AddScoped<ApplicationContext>()
            .AddTransient<IContextSetter, WebContextSetter>();
        return services;
    }

    private static IServiceCollection AddFactories(this IServiceCollection services)
    {
        return services
            .AddTransient<IAdapterFactory<IActiveQueueAdapter<ServiceBusMessage, ServiceBusReceivedMessage>>,
                AdapterFactory<IActiveQueueAdapter<ServiceBusMessage, ServiceBusReceivedMessage>>>()
            .AddTransient<IAdapterFactory<IDeadLetterQueueAdapter<ServiceBusMessage, ServiceBusReceivedMessage>>,
                AdapterFactory<IDeadLetterQueueAdapter<ServiceBusMessage, ServiceBusReceivedMessage>>>()
            .AddTransient<IAdapterFactory<IActiveTopicAdapter<ServiceBusMessage, ServiceBusReceivedMessage>>,
                AdapterFactory<IActiveTopicAdapter<ServiceBusMessage, ServiceBusReceivedMessage>>>()
            .AddTransient<IAdapterFactory<IDeadLetterTopicAdapter<ServiceBusMessage, ServiceBusReceivedMessage>>,
                AdapterFactory<IDeadLetterTopicAdapter<ServiceBusMessage, ServiceBusReceivedMessage>>>();
    }

    private static IServiceCollection AddAdapters(this IServiceCollection services)
    {
        return services
            .AddTransient<IActiveQueueAdapter<ServiceBusMessage, ServiceBusReceivedMessage>, ActiveQueueAdapter>()
            .AddTransient<IActiveTopicAdapter<ServiceBusMessage, ServiceBusReceivedMessage>, ActiveTopicAdapter>()
            .AddTransient<IDeadLetterQueueAdapter<ServiceBusMessage, ServiceBusReceivedMessage>, DeadLetterQueueAdapter>()
            .AddTransient<IDeadLetterTopicAdapter<ServiceBusMessage, ServiceBusReceivedMessage>, DeadLetterTopicAdapter>()
            .AddTransient<IRuntimeInfoAdapter, RuntimeInfoAdapter>();
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<IWorkerService>(new WorkerService())
            .AddTransient<INotificationService, NotificationService>()
            .AddTransient<IQueueService<ServiceBusMessage, ServiceBusReceivedMessage>, QueueService<ServiceBusMessage, ServiceBusReceivedMessage>>()
            .AddTransient<ITopicService<ServiceBusMessage, ServiceBusReceivedMessage>, TopicService<ServiceBusMessage, ServiceBusReceivedMessage>>()
            .AddTransient<IRuntimeInfoService, RuntimeInfoService>();
    }
}
