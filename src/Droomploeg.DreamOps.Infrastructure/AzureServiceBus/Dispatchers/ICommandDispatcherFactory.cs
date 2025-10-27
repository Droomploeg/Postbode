using Droomploeg.DreamOps.Application.Common;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Dispatchers;

/// <summary>
/// Command dispatcher factory interface.
/// </summary>
public interface ICommandDispatcherFactory
{
    /// <summary>
    /// Get <see cref="ICommandDispatcher"/> based on <see cref="ServiceBusConnection"/>
    /// </summary>
    /// <param name="connection"><see cref="ServiceBusConnection"/></param>
    /// <returns><see cref="ICommandDispatcher"/></returns>
    ICommandDispatcher GetDispatcher(ServiceBusConnection connection);
}
