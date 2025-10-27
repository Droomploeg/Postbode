using Droomploeg.DreamOps.Application.Common;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;

namespace Droomploeg.DreamOps.Application.ServiceBus.Factories;

/// <summary>
/// Dispatcher factory interface.
/// </summary>
public interface IDispatcherFactory
{
    /// <summary>
    /// Get <see cref="ICommandDispatcher"/> based on <see cref="ServiceBusConnection"/>
    /// </summary>
    /// <param name="connection"><see cref="ServiceBusConnection"/></param>
    /// <returns><see cref="ICommandDispatcher"/></returns>
    ICommandDispatcher GetDispatcher(ServiceBusConnection connection);
}
