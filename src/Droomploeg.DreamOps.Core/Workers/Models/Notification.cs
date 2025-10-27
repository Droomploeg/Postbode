using Droomploeg.DreamOps.Domain.Workers.Types;

namespace Droomploeg.DreamOps.Domain.Workers.Models;

public record Notification(
    Guid Id,
    DateTimeOffset Timestamp,
    string Entity,
    string Message,
    WorkItemState State)
{
    public NotificationType Type => State switch
        {
            WorkItemState.Created => NotificationType.Information,
            WorkItemState.Started => NotificationType.Information,
            WorkItemState.Completed => NotificationType.Completed,
            WorkItemState.Failed => NotificationType.Failure,
            WorkItemState.Cancelled => NotificationType.Information,
            _ => NotificationType.Information
        };
}

