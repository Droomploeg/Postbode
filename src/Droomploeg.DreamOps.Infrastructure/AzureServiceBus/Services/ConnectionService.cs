using Droomploeg.DreamOps.Application.ServiceBus.Services;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Services;

/// <summary>
/// Service bus connection manager
/// </summary>
/// <param name="connections"></param>
public class ConnectionService(IEnumerable<ServiceBusConnectionInfo> connections) : IConnectionService
{
    /// <inheritdoc cref="IConnectionService.Connections"/>
    public ServiceBusConnectionInfo[] Connections { get; } = connections.ToArray() ?? [];

    /// <inheritdoc cref="IConnectionService.GetConnection(ServiceBusConnection)"/>
    public ServiceBusConnectionInfo? GetConnection(ServiceBusConnection connection)
        => Connections.FirstOrDefault(c => c.Connection == connection);
}
