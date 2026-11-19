using Azure.Messaging.ServiceBus;
using Droomploeg.Postbode.Domain.ServiceBus.Types;

namespace Droomploeg.Postbode.Infrastructure.AzureServiceBus.Factories;

/// <summary>
/// Provides <see cref="ServiceBusClient"/> instances that act with the delegated token of the
/// signed-in user. A client belongs to exactly one user: the provider is scoped (one per
/// Blazor circuit), so connections and cached tokens are never shared between users.
/// </summary>
public interface IUserServiceBusClientProvider
{
    /// <summary>
    /// Returns the client for <paramref name="connection"/> for the current user, creating it on first use.
    /// </summary>
    /// <param name="connection">The configured Service Bus connection.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A client authorized with the current user's delegated token.</returns>
    ValueTask<ServiceBusClient> GetClientAsync(ServiceBusConnection connection, CancellationToken cancellationToken = default);
}
