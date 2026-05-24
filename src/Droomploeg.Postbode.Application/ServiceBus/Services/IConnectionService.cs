using Droomploeg.Postbode.Domain.ServiceBus.Types;

namespace Droomploeg.Postbode.Application.ServiceBus.Services;

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
