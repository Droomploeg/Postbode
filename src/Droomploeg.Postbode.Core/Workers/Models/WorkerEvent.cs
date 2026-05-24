using Droomploeg.Postbode.Domain.Workers.Types;

namespace Droomploeg.Postbode.Domain.Workers.Models;

/// <summary>
/// Worker event.
/// </summary>
/// <param name="State"><see cref="WorkItemState"/></param>
/// <param name="Exception"><see cref="Exception"/> when error occured</param>
public record WorkerEvent(WorkItemState State, Exception? Exception = null)
{
    private readonly DateTime _timestamp = DateTime.UtcNow;

    /// <summary>
    /// <see cref="DateTimeOffset"/> timestamp of the event.
    /// </summary>
    public DateTimeOffset Timestamp => _timestamp;
}

