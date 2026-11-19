using Azure.Core;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.Postbode.Domain.ServiceBus.Types;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Exceptions;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Factories;

namespace Droomploeg.Postbode.WebApp.Security;

/// <summary>
/// Singleton provider of the <see cref="ServiceBusAdministrationClient"/> per connection for the
/// service account. Sharing these clients between users is safe: they always act as the same
/// identity, and the administration client is a stateless HTTP client that needs no disposal.
/// </summary>
internal sealed partial class ServiceAccountAdminClientProvider : IServiceAccountAdminClientProvider
{
    private readonly IReadOnlyDictionary<string, ServiceBusAdministrationClient> _clients;
    private readonly ILogger<ServiceAccountAdminClientProvider> _logger;

    /// <param name="namespaces">Fully qualified namespace per connection name.</param>
    /// <param name="credential">Credential of the service account.</param>
    /// <param name="logger">Logger for this provider.</param>
    public ServiceAccountAdminClientProvider(
        IReadOnlyDictionary<string, string> namespaces,
        TokenCredential credential,
        ILogger<ServiceAccountAdminClientProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(namespaces);
        ArgumentNullException.ThrowIfNull(credential);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _clients = namespaces
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Value))
            .ToDictionary(
                entry => entry.Key,
                entry => new ServiceBusAdministrationClient(entry.Value, credential),
                StringComparer.OrdinalIgnoreCase);
    }

    public ServiceBusAdministrationClient GetClient(ServiceBusConnection connection)
    {
        if (_clients.TryGetValue(connection.Name, out var client))
        {
            return client;
        }

        LogUnknownConnection(connection.Name);
        throw new InvalidServiceBusConnectionException();
    }

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "No service account administration client configured for connection {ConnectionName}")]
    private partial void LogUnknownConnection(string connectionName);
}
