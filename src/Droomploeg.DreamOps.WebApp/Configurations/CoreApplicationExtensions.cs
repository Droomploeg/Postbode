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
            .AddRepostories()
            .AddServices();
    }

    private static IServiceCollection AddRepostories(this IServiceCollection services)
    {
        return services
            .AddTransient<IActiveQueueRepository<ServiceBusMessage, ServiceBusReceivedMessage>, ActiveQueueRepository>()
            .AddTransient<IActiveTopicRepository<ServiceBusMessage, ServiceBusReceivedMessage>, ActiveTopicRepository>()
            .AddTransient<IDeadLetterQueueRepository<ServiceBusMessage, ServiceBusReceivedMessage>, DeadLetterQueueRepository>()
            .AddTransient<IDeadLetterTopicRepository<ServiceBusMessage, ServiceBusReceivedMessage>, DeadLetterTopicRepository>()
            .AddTransient<IServiceBusRepository, ServiceBusRepository>();
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services
            .AddTransient<ActiveQueueService<ServiceBusMessage, ServiceBusReceivedMessage>>()
            .AddTransient<ActiveTopicService<ServiceBusMessage, ServiceBusReceivedMessage>>()
            .AddTransient<DeadLetterQueueService<ServiceBusMessage, ServiceBusReceivedMessage>>()
            .AddTransient<DeadLetterTopicService<ServiceBusMessage, ServiceBusReceivedMessage>>()
            .AddTransient<ServiceBusService>();
    }
}
