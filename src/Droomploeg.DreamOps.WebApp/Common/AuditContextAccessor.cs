using Droomploeg.DreamOps.Infrastructure.Audit.Disabled;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Droomploeg.DreamOps.WebApp.Common;

/// <summary>
/// Implementation of the <see cref="IAuditContextAccessor"/> that uses the <see cref="ProtectedSessionStorage"/> to store the current <see cref="IAuditContext"/>.
/// </summary>
/// <param name="sessionStorage"></param>
public sealed class AuditContextAccessor : IAuditContextAccessor
{
    private IAuditContext? _context;

    /// <see cref="IServiceBusConnectionAccessor.GetCurrentAsync"/>
    public Task<IAuditContext?> GetCurrentAsync()
    {
        return Task.FromResult(_context);
        //var result = await sessionStorage.GetAsync<IAuditContext>(nameof(IAuditContext));
        //return result.Success
        //    ? result.Value 
        //    : null;
    }

    /// <see cref="IServiceBusConnectionAccessor.SetCurrentAsync"/>
    public async Task SetCurrentAsync(IAuditContext? context)
    {
        //if (context == null)
        //{
        //    await sessionStorage.DeleteAsync(nameof(IAuditContext));
        //    return;
        //}

        //await sessionStorage.SetAsync(nameof(IAuditContext), context);
        _context = context;
        await Task.CompletedTask;
    }
}
