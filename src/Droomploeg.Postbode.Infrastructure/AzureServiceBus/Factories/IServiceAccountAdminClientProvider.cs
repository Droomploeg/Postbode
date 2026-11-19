using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.Postbode.Domain.ServiceBus.Types;

namespace Droomploeg.Postbode.Infrastructure.AzureServiceBus.Factories;

/// <summary>
/// Provides the <see cref="ServiceBusAdministrationClient"/> of the service account (managed identity).
/// The service account is only used to read metadata of queues, topics and subscriptions, which
/// requires the Data Owner role. Message operations always run with the token of the user.
/// </summary>
public interface IServiceAccountAdminClientProvider
{
    /// <summary>
    /// Gets the administration client for a connection.
    /// </summary>
    /// <param name="connection">The Service Bus connection.</param>
    /// <returns>The administration client of the service account for that connection.</returns>
    /// <exception cref="Exceptions.InvalidServiceBusConnectionException">When the connection is not configured.</exception>
    ServiceBusAdministrationClient GetClient(ServiceBusConnection connection);
}
