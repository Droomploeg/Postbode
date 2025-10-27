using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;

namespace Droomploeg.DreamOps.Application.ServiceBus.Services;

/// <summary>
/// Connection service to retrieve all configured Service Bus connections.
/// </summary>
public interface IConnectionService
{
    /// <summary>
    /// Get <see cref="IList{T}"/> of <see cref="ServiceBusConnectionInfo"/>
    /// </summary>
    ServiceBusConnectionInfo[] Connections { get; }

    /// <summary>
    /// Get <see cref="ServiceBusConnectionInfo"/> with <see cref="ServiceBusConnection"/>
    /// </summary>
    /// <param name="connection"><see cref="ServiceBusConnection"/></param>
    /// <returns><see cref="ServiceBusConnectionInfo"/> if found else null</returns>
    public ServiceBusConnectionInfo? GetConnection(ServiceBusConnection connection);
}
