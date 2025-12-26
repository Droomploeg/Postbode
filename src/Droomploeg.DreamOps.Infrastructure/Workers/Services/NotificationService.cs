using System;
using System.Linq;
using System.Transactions;
using Droomploeg.DreamOps.Application.Workers.Services;
using Droomploeg.DreamOps.Domain.Workers.Models;
using Droomploeg.DreamOps.Domain.Workers.Types;

namespace Droomploeg.DreamOps.Infrastructure.Workers.Services;

public class NotificationService(IWorkerService workerService) : INotificationService
{
    private readonly List<Notification> _popupNotifications = [];
    private readonly IWorkerService _workerService = workerService;
    private DateTimeOffset _lastPopupDateTimeOffset = DateTimeOffset.Now;

    public bool CleanUp(DateTimeOffset dateTimeOffset)
    {
        //var removeNotifications = _notifications
        //    .Where(n => n.Timestamp < dateTimeOffset)
        //    .ToList();

        //removeNotifications.ForEach(n => _notifications.Remove(n));

        //return removeNotifications.Count != 0;
        return false;
    }

    public bool TryUpdatePopupNotifications(IList<Notification> currentPopupNotifications, 
        DateTimeOffset dateTimeOffset, TimeSpan duration, 
        out IList<Notification> updatePopupNotifications)
    {
        var deleteTimestamp = dateTimeOffset.Add(duration);
        var activeNotifications = currentPopupNotifications.Where(n => n.Timestamp >= deleteTimestamp)
            .Select(wi => wi.Id);

        var newNotifications = _workerService.GetAll()
                .Where(n => _lastPopupDateTimeOffset.Ticks < n.UpdatedTimestamp.Ticks);

        var isUpdated = newNotifications.Any();
        if (isUpdated)
        { 
            _lastPopupDateTimeOffset = newNotifications.Max(n => n.UpdatedTimestamp).Add(TimeSpan.FromSeconds(-1));
        }
        
        var allPopupNotifications = activeNotifications.Union(newNotifications.Select(n => n.Id)).ToList();

        Console.WriteLine(allPopupNotifications.Count());

        updatePopupNotifications = [.. _workerService.GetAll()
            .Where(wi => allPopupNotifications.Contains(wi.Id))
            .OrderBy(wi => wi.GetEventDateTime(WorkItemState.Scheduled))
            .Select(wi => new Notification(
                wi.Id,
                wi.UpdatedTimestamp,
                wi.Entity,
                wi.Description,
                wi.State))];

        return isUpdated;
    }

    public void Remove(Guid id)
    {
        var index = _popupNotifications.FindIndex(n => n.Id == id);
        if (index > -1)
        {
            _popupNotifications.RemoveAt(index);
        }
    }

    public ICollection<Notification> GetAll()
        => [.. _popupNotifications.OrderByDescending(n => n.Timestamp)];
}
