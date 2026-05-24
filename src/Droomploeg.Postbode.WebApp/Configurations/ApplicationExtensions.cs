using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus;
using Droomploeg.Postbode.Application.ServiceBus.Adapters;
using Droomploeg.Postbode.Application.ServiceBus.Factories;
using Droomploeg.Postbode.Application.ServiceBus.Services;
using Droomploeg.Postbode.Application.Workers.Services;
using Droomploeg.Postbode.Infrastructure.Audit;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Adapters;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Factories;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Services;
using Droomploeg.Postbode.Infrastructure.Contexts;
using Droomploeg.Postbode.Infrastructure.Workers.Services;

namespace Droomploeg.Postbode.WebApp.Configurations;

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
