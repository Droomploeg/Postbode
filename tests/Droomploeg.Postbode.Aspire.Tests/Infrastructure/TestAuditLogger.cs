using Droomploeg.Postbode.Infrastructure.Audit;

namespace Droomploeg.Postbode.Aspire.Tests.Infrastructure;

/// <summary>
/// Test implementation of IAuditLogger that captures audit entries for assertion in tests.
/// Thread-safe for use across multiple test scenarios.
/// </summary>
public class TestAuditLogger : IAuditLogger
{
    private readonly List<AuditEntry> _entries = [];
    private readonly Lock _sync = new();

    public IReadOnlyList<AuditEntry> Entries
    {
        get
        {
            lock (_sync) return [.. _entries];
        }
    }

    public Task LogAuditAsync(string action, string resource, string? details = null)
    {
        lock (_sync)
        {
            _entries.Add(new AuditEntry(action, resource, details));
        }
        return Task.CompletedTask;
    }

    public void Clear()
    {
        lock (_sync) _entries.Clear();
    }

    public record AuditEntry(string Action, string Resource, string? Details);
}
