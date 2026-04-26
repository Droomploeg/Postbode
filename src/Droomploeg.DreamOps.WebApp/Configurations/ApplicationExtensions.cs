using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Application.ServiceBus.Factories;
using Droomploeg.DreamOps.Application.ServiceBus.Services;
using Droomploeg.DreamOps.Application.Workers.Services;
using Droomploeg.DreamOps.Infrastructure.Audit;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Adapters;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Factories;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Services;
using Droomploeg.DreamOps.Infrastructure.Contexts;
using Droomploeg.DreamOps.Infrastructure.Workers.Services;

namespace Droomploeg.DreamOps.WebApp.Configurations;

[ExcludeFromCodeCoverage( Justification = "Application extensions")]
internal static class ApplicationExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddApplicationCore() => services.AddSingleton(TimeProvider.System)
            .AddContext()
            .AddFactories()
            .AddAdapters()
            .AddServices();

        private IServiceCollection AddContext()
        {
            services
                .AddScoped<ApplicationContext>()
                .AddTransient<IContextSetter, WebContextSetter>();
            return services;
        }

        private IServiceCollection AddFactories()
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

        private IServiceCollection AddAdapters()
        {
            return services
                .AddTransient<ISessionInfoProvider, SessionInfoProvider>()
                .AddTransient<IActiveQueueAdapter<ServiceBusMessage, ServiceBusReceivedMessage>, ActiveQueueAdapter>()
                .AddTransient<IActiveTopicAdapter<ServiceBusMessage, ServiceBusReceivedMessage>, ActiveTopicAdapter>()
                .AddTransient<IDeadLetterQueueAdapter<ServiceBusMessage, ServiceBusReceivedMessage>, DeadLetterQueueAdapter>()
                .AddTransient<IDeadLetterTopicAdapter<ServiceBusMessage, ServiceBusReceivedMessage>, DeadLetterTopicAdapter>()
                .AddTransient<IRuntimeInfoAdapter, RuntimeInfoAdapter>();
        }

        private IServiceCollection AddServices()
        {
            return services
                .AddSingleton<IWorkerService, WorkerService>()
                .AddTransient<IAuditLogger, AuditLogger>()
                .AddTransient<INotificationService, NotificationService>()
                .AddTransient<IQueueService<ServiceBusMessage, ServiceBusReceivedMessage>, QueueService<ServiceBusMessage, ServiceBusReceivedMessage>>()
                .AddTransient<ITopicService<ServiceBusMessage, ServiceBusReceivedMessage>, TopicService<ServiceBusMessage, ServiceBusReceivedMessage>>()
                .AddTransient<IRuntimeInfoService, RuntimeInfoService>();
        }
    }
}
