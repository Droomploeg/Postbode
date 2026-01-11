namespace Droomploeg.DreamOps.Infrastructure.Audit;

public interface IAuditContext
{
    string CorrelationId { get; }
    string? UserId { get; }
    string? UserName { get; }
    string? Path { get; }
    string? RemoteIp { get; }
    string? UserAgent { get; }
    string? TenantId { get; }
    IDictionary<string, string>? Properties { get; }
}
