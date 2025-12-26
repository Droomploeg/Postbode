using Droomploeg.DreamOps.Application.ServiceBus.Services;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Services;

/// <summary>
/// Service bus connection manager
/// </summary>
/// <param name="connections"></param>
public class ConnectionService(IEnumerable<ServiceBusConnection> connections) : IConnectionService
{
    /// <inheritdoc cref="IConnectionService.Connections"/>
    public ServiceBusConnection[] Connections { get; } = connections.ToArray() ?? [];
}
