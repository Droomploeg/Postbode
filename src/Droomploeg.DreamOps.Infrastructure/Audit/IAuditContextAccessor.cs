namespace Droomploeg.DreamOps.Infrastructure.Audit;

public interface IAuditContextAccessor
{
    IAuditContext Current { get; set; }
}

