namespace Droomploeg.DreamOps.Domain.Workers.Types;

/// <summary>
/// Notifucation types.
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
