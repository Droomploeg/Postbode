using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Droomploeg.DreamOps.Infrastructure.Contexts;
using Microsoft.Extensions.Azure;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Extensions;

/// <summary>
/// Extension methods for <see cref="IAzureClientFactory{TClient}"/>.
/// </summary>
public static class AzureClientFactoryExtensions
{
    /// <summary>
    /// Creates a <see cref="ServiceBusAdministrationClient"/> using the current application context connection.
    /// </summary>
    /// <param name="factory">The Azure client factory.</param>
    /// <param name="context">The application context providing the current connection.</param>
    /// <returns>A configured <see cref="ServiceBusAdministrationClient"/>.</returns>
    public static ServiceBusAdministrationClient CreateClient(this IAzureClientFactory<ServiceBusAdministrationClient> factory, ApplicationContext context)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(context);

        var clientName = context.CurrentConnection.ClientName(ServiceBusConnectionType.ServiceAccount);

        return factory.CreateClient(clientName);

    }

    /// <summary>
    /// Creates a <see cref="ServiceBusClient"/> using the current application context connection and specified connection type.
    /// </summary>
    /// <param name="factory">The Azure client factory.</param>
    /// <param name="context">The application context providing the current connection.</param>
    /// <param name="connectionType">The connection type (e.g., service account or on-behalf-of).</param>
    /// <returns>A configured <see cref="ServiceBusClient"/>.</returns>
    public static ServiceBusClient CreateClient(this IAzureClientFactory<ServiceBusClient> factory, ApplicationContext context, ServiceBusConnectionType connectionType)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(context);
        
        var clientName = context.CurrentConnection.ClientName(connectionType);

        return factory.CreateClient(clientName);
    }
}

