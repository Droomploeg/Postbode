namespace Droomploeg.Postbode.Domain.Workers.Types;

/// <summary>
/// Notification types.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Information notification.
    /// </summary>
    Information,

    /// <summary>
    /// Failure notification.
    /// </summary>
    Failure,

    /// <summary>
    /// Warning notification.
    /// </summary>
    Warning,

    /// <summary>
    /// Completed notification.
    /// </summary>
    Completed,
}
