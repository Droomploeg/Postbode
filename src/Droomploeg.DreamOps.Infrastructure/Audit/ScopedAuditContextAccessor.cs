namespace Droomploeg.DreamOps.Infrastructure.Audit;

public sealed class ScopedAuditContextAccessor : IAuditContextAccessor
{
    private IAuditContext? _context;

    public ValueTask SetCurrentAsync(IAuditContext? context)
    {
        _context = context;
        return ValueTask.CompletedTask;
    }

    public ValueTask<IAuditContext?> GetCurrentAsync()
    {
        return ValueTask.FromResult(_context);
    }
}
