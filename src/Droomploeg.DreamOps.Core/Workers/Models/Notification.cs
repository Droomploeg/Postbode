using Droomploeg.DreamOps.Domain.Workers.Types;

namespace Droomploeg.DreamOps.Domain.Workers.Models;

/// <summary>
/// Notification record.
/// </summary>
/// <param name="Id">Id of the notification</param>
/// <param name="Entity">Entity</param>
/// <param name="Message">Message</param>
/// <param name="TimestampCreated"><see cref="DateTimeOffset"/> of creation of the notification</param>
/// <param name="TimestampStateChanged"><see cref="DateTimeOffset"/> of last update of the notification</param>
/// <param name="State"><see cref="WorkItemState"/></param>
public record Notification(
    Guid Id,
    string Entity,
    string Message,
    DateTimeOffset TimestampCreated,
    DateTimeOffset TimestampStateChanged,
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
            WorkItemState.Cancelled => NotificationType.Warning,
            _ => NotificationType.Information
        };
}

