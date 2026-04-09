namespace Droomploeg.DreamOps.Infrastructure.Audit;

/// <summary>
/// Audit the logger interface for tracking user actions.
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Logs an audit entry with user, action, and context information.
    /// </summary>
    /// <param name="action">The action being performed</param>
    /// <param name="resource">The resource being acted upon (e.g., queue name, topic/subscription)</param>
    /// <param name="details">Additional details about the action</param>
    /// <returns><see cref="Task"/></returns>
    Task LogAuditAsync(string action, string resource, string? details = null);
}
