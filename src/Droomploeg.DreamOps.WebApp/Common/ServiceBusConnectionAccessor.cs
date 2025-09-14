using Droomploeg.DreamOps.Infrastructure.AzureServiceBus;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Droomploeg.DreamOps.WebApp.Common;

/// <summary>
/// Implementation of the <see cref="IServiceBusConnectionAccessor"/> to store the current selected <see cref="ServiceBusConnection"/> in the session storage.
/// </summary>
/// <param name="sessionStorage"></param>
public class ServiceBusConnectionAccessor(ProtectedSessionStorage sessionStorage) : IServiceBusConnectionAccessor
{
    /// <see cref="IServiceBusConnectionAccessor.GetCurrentAsync"/>
    public async Task<ServiceBusConnection> GetCurrentAsync()
    {
        var result = await sessionStorage.GetAsync<ServiceBusConnection>(nameof(ServiceBusConnection));
        return result.Success
            ? result.Value ?? ServiceBusConnection.None
            : ServiceBusConnection.None;
    }

    /// <see cref="IServiceBusConnectionAccessor.SetCurrentAsync"/>
    public async Task SetCurrentAsync(ServiceBusConnection connection)
    {
        await sessionStorage.SetAsync(nameof(ServiceBusConnection), connection);
    }
}
