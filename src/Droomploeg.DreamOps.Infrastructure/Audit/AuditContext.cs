namespace Droomploeg.DreamOps.Infrastructure.Audit;

public sealed record AuditContext : IAuditContext
{
    public string CorrelationId { get; init; } = string.Empty;
    public string? UserId { get; init; }
    public string? UserName { get; init; }
    public string? Path { get; init; }
    public string? RemoteIp { get; init; }
    public string? UserAgent { get; init; }
    public string? TenantId { get; init; }
    public IDictionary<string, string>? Properties { get; init; }
}
