using Droomploeg.DreamOps.Domain.Workers.Models;

namespace Droomploeg.DreamOps.Application.Workers.Services;

/// <summary>
/// Notification service interface.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Try to update popup notifications.
    /// </summary>
    /// <param name="inputNotifications">Current <see cref="IList{T}"/> of <see cref="Notification"/></param>
    /// <param name="currentTimestamp"><see cref="DateTimeOffset"/></param>
    /// <param name="duration"><see cref="TimeSpan"/></param>
    /// <param name="outputNotifications">New <see cref="IList{T}"/> of <see cref="Notification"/></param>
    /// <returns><see langword="true"/> if there are popup notifications changed</returns>
    bool TryUpdatePopupNotifications(IList<Notification> inputNotifications, 
        DateTimeOffset currentTimestamp, TimeSpan duration, 
        out IList<Notification> outputNotifications);
    
    /// <summary>
    /// Update notifications.
    /// </summary>
    /// <param name="currentNotifications">Current <see cref="IList{T}"/> of <see cref="Notification"/></param>
    /// <param name="currentTimestamp"><see cref="DateTimeOffset"/></param>
    /// <returns>New <see cref="IList{T}"/> of <see cref="Notification"/></returns>
    IList<Notification> UpdateNotifications(IList<Notification> currentNotifications, 
        DateTimeOffset currentTimestamp);

}
