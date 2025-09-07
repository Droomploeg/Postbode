namespace Droomploeg.DreamOps.Infrastructure.Audit;

public sealed class AuditContextAccessor : IAuditContextAccessor
{
    public IAuditContext Current { get; set; } = new AuditContext { CorrelationId = Guid.NewGuid().ToString("n") };
}
