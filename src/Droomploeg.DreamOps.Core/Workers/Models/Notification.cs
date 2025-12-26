using Droomploeg.DreamOps.Domain.Workers.Types;

namespace Droomploeg.DreamOps.Domain.Workers.Models;

/// <summary>
/// Notification record.
/// </summary>
/// <param name="Id">Id of the notification</param>
/// <param name="Timestamp"><see cref="DateTimeOffset"> of last update of the notification</param>
/// <param name="Entity">Entity</param>
/// <param name="Message">Message</param>
/// <param name="State"><see cref="WorkItemState"/></param>
public record Notification(
    Guid Id,
    DateTimeOffset Timestamp,
    string Entity,
    string Message,
    WorkItemState State)
{
    /// <summary>
    /// Type of the notification.
    /// </summary>
    public NotificationType Type => State switch
        {
            WorkItemState.Scheduled => NotificationType.Information,
            WorkItemState.Started => NotificationType.Information,
            WorkItemState.Completed => NotificationType.Completed,
            WorkItemState.Failed => NotificationType.Failure,
            WorkItemState.Cancelled => NotificationType.Information,
            _ => NotificationType.Information
        };
}

