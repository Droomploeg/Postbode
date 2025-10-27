using Azure.Messaging.ServiceBus;
using Droomploeg.DreamOps.Application.Common;
using Droomploeg.DreamOps.Application.ServiceBus.Commands.Queues;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Dispatchers;
using ManagedIdentity = Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Handlers.ManagedIdentities.Queues;
using OnBehalfOf = Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Handlers.OnBehalfOf.Queues;

namespace Droomploeg.DreamOps.WebApp.Configurations;

internal static class ServiceBusCommandsExtensions
{
    internal static IServiceCollection AddCommandCore(this IServiceCollection services)
    {
        return services
            .AddFactories()
            .AddDispatchers()
            .AddManagedIdentityQueueCommands()
            .AddOnBehalfOfQueueCommands();
    }


    private static IServiceCollection AddFactories(this IServiceCollection services)
    {
        return services
            .AddTransient<ICommandDispatcherFactory, CommandDispatcherFactory>();
    }

    private static IServiceCollection AddDispatchers(this IServiceCollection services)
    {
        return services
            .AddKeyedTransient<ICommandDispatcher, ManagedIdentityDispatcher>(CommandDispatcherFactory.ManagedIdentity)
            .AddKeyedTransient<ICommandDispatcher, OnBehalfOfDispatcher>(CommandDispatcherFactory.OnBehalfOf);
    }

    private static IServiceCollection AddManagedIdentityQueueCommands(this IServiceCollection services)
    {
        var managedIdentity = CommandDispatcherFactory.ManagedIdentity;

        // active queue handlers
        services
            .AddKeyedTransient<ICommandHandler<SendMessageCommand<ServiceBusMessage>>,
                ManagedIdentity.SendMessageCommandHandler<ServiceBusMessage, ServiceBusReceivedMessage>>(managedIdentity)
            .AddKeyedTransient<ICommandHandler<DeleteActiveMessageCommand<ServiceBusReceivedMessage>>,
                ManagedIdentity.DeleteActiveMessageCommandHandler<ServiceBusMessage, ServiceBusReceivedMessage>>(managedIdentity)
            .AddKeyedTransient<ICommandHandler<DeleteAllActiveMessagesCommand>,
                ManagedIdentity.DeleteAllActiveMessagesCommandHandler<ServiceBusMessage, ServiceBusReceivedMessage>>(managedIdentity)
            .AddKeyedTransient<ICommandHandler<DeadLetterMessageCommand<ServiceBusReceivedMessage>>,
                ManagedIdentity.DeadLetterMessageCommand<ServiceBusMessage, ServiceBusReceivedMessage>>(managedIdentity);
        // dead-letter queue handlers
        services
            .AddKeyedTransient<ICommandHandler<ResubmitMessageCommand<ServiceBusMessage, ServiceBusReceivedMessage>>,
                ManagedIdentity.ResubmitMessageCommandHandler<ServiceBusMessage, ServiceBusReceivedMessage>>(managedIdentity)
            .AddKeyedTransient<ICommandHandler<ResubmitAllMessagesCommand>,
                ManagedIdentity.ResubmitAllMessagesCommandHandler<ServiceBusMessage, ServiceBusReceivedMessage>>(managedIdentity)
            .AddKeyedTransient<ICommandHandler<DeleteDeadLetterMessageCommand<ServiceBusReceivedMessage>>,
                ManagedIdentity.DeleteDeadLetterMessageCommandHandler<ServiceBusMessage, ServiceBusReceivedMessage>>(managedIdentity)
            .AddKeyedTransient<ICommandHandler<DeleteAllDeadLetterMessagesCommand>,
                ManagedIdentity.DeleteAllDeadLetterMessagesCommandHandler<ServiceBusMessage, ServiceBusReceivedMessage>>(managedIdentity);

        return services;
    }

    private static IServiceCollection AddOnBehalfOfQueueCommands(this IServiceCollection services)
    {
        var onBehalfOf = CommandDispatcherFactory.OnBehalfOf;

        // active queue handlers
        services
            .AddKeyedTransient<ICommandHandler<SendMessageCommand<ServiceBusMessage>>,
                OnBehalfOf.SendMessageCommandHandler<ServiceBusMessage, ServiceBusReceivedMessage>>(onBehalfOf)
            .AddKeyedTransient<ICommandHandler<DeleteActiveMessageCommand<ServiceBusReceivedMessage>>,
                OnBehalfOf.DeleteActiveMessageCommandHandler<ServiceBusMessage, ServiceBusReceivedMessage>>(onBehalfOf)
            .AddKeyedTransient<ICommandHandler<DeadLetterMessageCommand<ServiceBusReceivedMessage>>,
                OnBehalfOf.DeadLetterMessageCommand<ServiceBusMessage, ServiceBusReceivedMessage>>(onBehalfOf);
        // dead-letter queue handlers
        services
            .AddKeyedTransient<ICommandHandler<ResubmitMessageCommand<ServiceBusMessage, ServiceBusReceivedMessage>>,
                OnBehalfOf.ResubmitMessageCommandHandler<ServiceBusMessage, ServiceBusReceivedMessage>>(onBehalfOf)
            .AddKeyedTransient<ICommandHandler<DeleteDeadLetterMessageCommand<ServiceBusReceivedMessage>>,
                OnBehalfOf.DeleteDeadLetterMessageCommandHandler<ServiceBusMessage, ServiceBusReceivedMessage>>(onBehalfOf);

        return services;
    }


}
