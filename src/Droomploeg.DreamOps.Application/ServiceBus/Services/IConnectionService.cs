using Droomploeg.DreamOps.Domain.ServiceBus.Types;

namespace Droomploeg.DreamOps.Application.ServiceBus.Services;

/// <summary>
/// Connection service to retrieve all configured Service Bus connections.
/// </summary>
public interface IConnectionService
{
    /// <summary>
    /// Get <see cref="IList{T}"/> of <see cref="ServiceBusConnection"/>
    /// </summary>
    ServiceBusConnection[] Connections { get; }
}
