// This file was archived and moved to src/obsolete/audit/AuditContextAccessor.cs
// If you need the original implementation, find it in the archive folder.

using Droomploeg.DreamOps.Infrastructure.Audit;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Droomploeg.DreamOps.WebApp.Common;

/// <summary>
/// Implementation of the <see cref="IAuditContextAccessor"/> that uses the <see cref="ProtectedSessionStorage"/> to store the current <see cref="IAuditContext"/>.
/// </summary>
/// <param name="sessionStorage"></param>
[Obsolete("This AuditContextAccessor used to store audit context in ProtectedSessionStorage for Blazor. The project now uses ScopedAuditContextAccessor from the Infrastructure project by default. File left as obsolete placeholder and can be deleted if you are sure it's not needed.")]
public sealed class AuditContextAccessor : IAuditContextAccessor
{
    private IAuditContext? _context;

    /// <see cref="IAuditContextAccessor.GetCurrentAsync"/>
    public ValueTask<IAuditContext?> GetCurrentAsync()
    {
        return ValueTask.FromResult(_context);
        //var result = await sessionStorage.GetAsync<IAuditContext>(nameof(IAuditContext));
        //return result.Success
        //    ? result.Value 
        //    : null;
    }

    /// <see cref="IAuditContextAccessor.SetCurrentAsync"/>
    public async ValueTask SetCurrentAsync(IAuditContext? context)
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
