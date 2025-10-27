using Droomploeg.DreamOps.Domain.Workers.Types;

namespace Droomploeg.DreamOps.Domain.Workers.Models;

public record WorkerAction(string User, WorkItemAction Action)
{
    private readonly DateTime _timestamp = DateTime.UtcNow;

    public DateTimeOffset Timestamp => _timestamp;
}

