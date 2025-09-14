namespace Droomploeg.DreamOps.Infrastructure.Audit;

public interface IAuditLogger
{
    Task DataChange(string action, string serviceBus, string entity, string? entityId, object? changes = null);
    Task DomainAction(string action, string serviceBus, string? entity = null, string? entityId = null, object? data = null);
}
