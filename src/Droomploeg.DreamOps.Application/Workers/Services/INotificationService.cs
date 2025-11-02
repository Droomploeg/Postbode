using Droomploeg.DreamOps.Domain.Workers.Models;

namespace Droomploeg.DreamOps.Application.Workers.Services;

/// <summary>
/// Notification service interface.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Update
    /// </summary>
    /// <param name="dateTimeOffset"><see cref="DateTimeOffset"/></param>
    /// <returns><see langword="true"/> after updated</returns>
    bool Update(DateTimeOffset dateTimeOffset);


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
