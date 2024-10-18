using Azure.Messaging.ServiceBus;
using Droomploeg.DreamOps.Core.Repositories;
using Droomploeg.DreamOps.Core.Services;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Repositories;

namespace Droomploeg.DreamOps.WebApp.Configurations;

public static class CoreApplicationExtensions
{
    public static IServiceCollection AddApplicationCore(this IServiceCollection services)
    {
        return services.AddSingleton(TimeProvider.System)
            .AddTransient<IServiceBusRepository, ServiceBusRepository>()
            .AddTransient<IQueueRepository<ServiceBusMessage, ServiceBusReceivedMessage>, QueueRepository>()
            .AddTransient<ITopicRepository<ServiceBusMessage, ServiceBusReceivedMessage>, TopicRepository>()
            .AddTransient<ServiceBusService>()
            .AddTransient<QueueService<ServiceBusMessage, ServiceBusReceivedMessage>>()
            .AddTransient<TopicService<ServiceBusMessage, ServiceBusReceivedMessage>>();
    }
}
