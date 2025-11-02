using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Exceptions;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Droomploeg.DreamOps.Infrastructure.Contexts;

/// <summary>
/// Web context setter to set the current application context based on the protected session storage.
/// </summary>
public class WebContextSetter : IContextSetter
{
    private readonly ProtectedSessionStorage _protectedSessionStorage;
    private readonly ApplicationContext _context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="protectedSessionStorage"><see cref="ProtectedSessionStorage"/></param>
    /// <param name="context"><see cref="ApplicationContext"/></param>
    /// <exception cref="ArgumentNullException">Occurs when protectedSessionStorage or context is null</exception>
    public WebContextSetter(ProtectedSessionStorage protectedSessionStorage, ApplicationContext context)
    {
        _protectedSessionStorage = protectedSessionStorage ?? throw new ArgumentNullException(nameof(protectedSessionStorage));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc cref="IContextSetter.GetAndUpdateAsync"/>
    /// <exception cref="InvalidServiceBusConnectionException"></exception>
    public async Task<ApplicationContext> GetAndUpdateAsync()
    {
        var result = await _protectedSessionStorage.GetAsync<ServiceBusConnectionInfo>(nameof(ServiceBusConnectionInfo));
        if (!result.Success || result.Value is null || result.Value.Connection.IsNotDefined)
        {
            throw new InvalidServiceBusConnectionException();
        }

        _context.CurrentConnection = result.Value.Connection;
        _context.CurrentConnectionType = ServiceBusConnectionType.UserAccount;

        return _context;
    }
}
