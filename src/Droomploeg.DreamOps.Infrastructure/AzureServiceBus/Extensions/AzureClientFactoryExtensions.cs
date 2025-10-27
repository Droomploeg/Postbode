using System.Net.Sockets;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Application;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Droomploeg.DreamOps.Infrastructure.Contexts;
using Microsoft.Extensions.Azure;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Extensions;

public static class AzureClientFactoryExtensions
{
    public static ServiceBusAdministrationClient CreateClient(this IAzureClientFactory<ServiceBusAdministrationClient> factory, ApplicationContext context)
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var clientName = context.CurrentConnection.ClientName(context.CurrentConnectionType); 

        return factory.CreateClient(clientName);

    }

    public static ServiceBusClient CreateClient(this IAzureClientFactory<ServiceBusClient> factory, ApplicationContext context)
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var clientName = context.CurrentConnection.ClientName(context.CurrentConnectionType);

        return factory.CreateClient(clientName);
    }
}

