namespace Droomploeg.DreamOps.Infrastructure.Audit;

public interface IAuditService
{
    /// <summary>
    /// Log a structured audit event. Must not throw.
    /// </summary>
    ValueTask LogEventAsync(string name, IAuditContext? context = null, IDictionary<string, string?>? properties = null, CancellationToken cancellationToken = default);
}
