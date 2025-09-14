namespace Droomploeg.DreamOps.Infrastructure.Audit;

/// <summary>
/// Interface of accessor for <see cref="IAuditContext"/>.
/// </summary>
public interface IAuditContextAccessor
{
    /// <summary>
    /// Set current <see cref="IAuditContext"/>.
    /// </summary>
    /// <returns><see cref="Task"></returns>
    Task SetCurrentAsync(IAuditContext? context);

    /// <summary>
    /// Get current <see cref="IAuditContext"/>.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="IAuditContext"/></returns>
    Task<IAuditContext?> GetCurrentAsync();
}

