namespace Droomploeg.DreamOps.Infrastructure.Audit;

public interface IAuditContextAccessor
{
    ValueTask SetCurrentAsync(IAuditContext? context);
    ValueTask<IAuditContext?> GetCurrentAsync();
}
