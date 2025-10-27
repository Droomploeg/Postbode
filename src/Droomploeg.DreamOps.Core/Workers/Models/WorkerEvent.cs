using Droomploeg.DreamOps.Domain.Workers.Types;

namespace Droomploeg.DreamOps.Domain.Workers.Models;

public record WorkerEvent(WorkItemState State, Exception? Exception = null)
{
    private readonly DateTime _timestamp = DateTime.UtcNow;

    public DateTimeOffset Timestamp => _timestamp;
}

