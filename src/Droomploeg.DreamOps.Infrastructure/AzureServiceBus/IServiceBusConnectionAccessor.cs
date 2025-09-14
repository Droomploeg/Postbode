namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus;

/// <summary>
/// Interface to access current <see cref="ServiceBusConnection"/>.
/// </summary>
public interface IServiceBusConnectionAccessor
{
    /// <summary>
    /// Set current <see cref="ServiceBusConnection"/>.
    /// </summary>
    /// <param name="connection"><see cref="ServiceBusConnection"/></param>
    /// <returns><see cref="Task"></returns>
    Task SetCurrentAsync(ServiceBusConnection connection);

    /// <summary>
    /// Get current <see cref="ServiceBusConnection"/>.
    /// </summary>
    /// <returns><see cref="ServiceBusConnection"/></returns>
    Task<ServiceBusConnection> GetCurrentAsync();
}
