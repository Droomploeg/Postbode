namespace Droomploeg.DreamOps.Infrastructure.Audit;

public interface IAuditLogger
{
    void DataChange(string action, string serviceBus, string entity, string? entityId, object? changes = null);
    void DomainAction(string action, string serviceBus, string? entity = null, string? entityId = null, object? data = null);
}
