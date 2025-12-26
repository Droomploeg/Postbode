using System.Collections;
using Droomploeg.DreamOps.Domain.Workers.Models;

namespace Droomploeg.DreamOps.Application.Workers.Services;

/// <summary>
/// Notification service interface.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Try get Pop-Up Notification.
    /// </summary>
    /// <param name="currentNotifications">Current <see cref="IList{T}"/> of <see cref="Notification"/></param>
    /// <param name="dateTimeOffset"><see cref="DateTimeOffset"/></param>
    /// <param name="durration"><see cref="TimeSpan"/></param>
    /// <param name="newNotifications">New <see cref="IList{T}"/> of <see cref="Notification"/></param>
    /// <returns><see langword="true"/> if there are notifications</returns>
    bool TryUpdatePopupNotifications(IList<Notification> currentNotifications, 
        DateTimeOffset dateTimeOffset, TimeSpan durration, 
        out IList<Notification> newNotifications);

    /// <summary>
    /// Cleanup.
    /// </summary>
    /// <param name="dateTimeOffset"><see cref="DateTimeOffset"/></param>
    /// <returns><see langword="true"/> after clean up</returns>
    bool CleanUp(DateTimeOffset dateTimeOffset);

    /// <summary>
    /// Remove notification by Id.
    /// </summary>
    /// <param name="Id"><see cref="Guid"/></param>
    void Remove(Guid Id);

    /// <summary>
    /// Get all notifications.
    /// </summary>
    /// <returns></returns>
    ICollection<Notification> GetAll();
}
