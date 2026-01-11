using Droomploeg.DreamOps.Application.Workers.Services;
using Droomploeg.DreamOps.Domain.Workers.Models;
using Droomploeg.DreamOps.Domain.Workers.Types;

namespace Droomploeg.DreamOps.Infrastructure.Workers.Services;

public class NotificationService(IWorkerService workerService) : INotificationService
{
    private readonly IWorkerService _workerService = workerService ?? throw new ArgumentNullException(nameof(workerService));
    
    public bool TryUpdatePopupNotifications(IList<Notification> inputNotifications, 
        DateTimeOffset currentTimestamp, TimeSpan duration, 
        out IList<Notification> outputNotifications)
    {
        var expiredTimestamp = currentTimestamp.Add(duration);

        var activeNotifications = inputNotifications
            .Where(n => n.TimestampStateChanged - expiredTimestamp > TimeSpan.FromSeconds(0))
            .Select(wi => wi.Id)
            .ToList();

        var newNotifications = _workerService.GetAll()
                .Where(n => currentTimestamp - n.UpdatedTimestamp < TimeSpan.FromSeconds(1))
                .ToList();

        var isUpdated = newNotifications.Count != 0 || inputNotifications.Count != activeNotifications.Count;
        
        var allPopupNotifications = activeNotifications
            .Union(newNotifications.Select(n => n.Id))
            .ToList();

        outputNotifications = [.. _workerService.GetAll()
            .Where(wi => allPopupNotifications.Contains(wi.Id))
            .Select(wi => new Notification(
                wi.Id,
                wi.Entity,
                wi.Description,
                wi.CreatedTimestamp,
                wi.UpdatedTimestamp,
                wi.State))];

        return isUpdated;
    }

    
    public IList<Notification> UpdateNotifications(IList<Notification> currentNotifications, 
        DateTimeOffset currentTimestamp)
    {
        var newNotifications = _workerService.GetAll()
            .Where(n => currentTimestamp - n.UpdatedTimestamp < TimeSpan.FromSeconds(1))
            .ToList();

        var allNotifications = currentNotifications
            .Where(n => !IsCompletedAndExpired(n, currentTimestamp))
            .Select(n => n.Id)
            .Union(newNotifications.Select(n => n.Id))
            .ToList();

        return [.. _workerService.GetAll()
            .Where(wi => allNotifications.Contains(wi.Id))
            .Select(wi => new Notification(
                wi.Id,
                wi.Entity,
                wi.Description,
                wi.CreatedTimestamp,
                wi.UpdatedTimestamp,
                wi.State))];
    }

    private bool IsCompletedAndExpired(Notification notification, DateTimeOffset currentTimestamp)
    {
        return (notification.State is WorkItemState.Completed or WorkItemState.Failed or WorkItemState.Cancelled)
            && (currentTimestamp - notification.TimestampStateChanged) > TimeSpan.FromSeconds(5);
    }
}
