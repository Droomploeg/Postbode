using Droomploeg.Postbode.Domain.Workers.Types;

namespace Droomploeg.Postbode.Domain.Workers.Models;

/// <summary>
/// Worker action.
/// </summary>
/// <param name="UserName">Username</param>
/// <param name="Action"><see cref="WorkItemAction"/></param>
public record WorkerAction(string UserName, WorkItemAction Action)
{
    private readonly DateTime _timestamp = DateTime.UtcNow;

    /// <summary>
    /// <see cref="DateTimeOffset"/> of the action.
    /// </summary>
    public DateTimeOffset Timestamp => _timestamp;
}

