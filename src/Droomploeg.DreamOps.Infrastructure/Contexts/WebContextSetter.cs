using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Exceptions;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Droomploeg.DreamOps.Infrastructure.Contexts;

public class WebContextSetter
{
    private readonly ProtectedSessionStorage _protectedSessionStorage;
    private readonly ApplicationContext _context;

    public WebContextSetter(ProtectedSessionStorage protectedSessionStorage, ApplicationContext context)
    {
        _protectedSessionStorage = protectedSessionStorage ?? throw new ArgumentNullException(nameof(protectedSessionStorage));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

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
