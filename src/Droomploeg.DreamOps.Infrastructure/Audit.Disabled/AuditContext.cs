namespace Droomploeg.DreamOps.Infrastructure.Audit.Disabled;

public sealed class AuditContext: IAuditContext
{
    public required string CorrelationId { get; init; }
    public string? UserId { get; init; }
    public string? UserName { get; init; }
    public string? Path { get; init; }
    public string? RemoteIp { get; init; }
    public string? UserAgent { get; init; }
    public string? TenantId { get; init; }
}
