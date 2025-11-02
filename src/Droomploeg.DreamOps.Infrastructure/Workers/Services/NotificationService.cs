using Droomploeg.DreamOps.Application.Workers.Services;
using Droomploeg.DreamOps.Domain.Workers.Models;

namespace Droomploeg.DreamOps.Infrastructure.Workers.Services;

public class NotificationService(IWorkerService workerService) : INotificationService
{
    private readonly List<Notification> _notifications = [];
    private readonly IWorkerService _workerService = workerService;

    public bool CleanUp(DateTimeOffset dateTimeOffset)
    {
        var removeNotifications = _notifications
            .Where(n => n.Timestamp < dateTimeOffset)
            .ToList();

        removeNotifications.ForEach(n => _notifications.Remove(n));

        return removeNotifications.Count != 0;
    }

    public bool Update(DateTimeOffset dateTimeOffset)
    {
        var newNotifications = _workerService.GetAll()
            .Where(wi => wi.UpdatedTimestamp > dateTimeOffset)
            .Select(wi => new Notification(
                wi.Id,
                wi.UpdatedTimestamp,
                wi.Entity,
                wi.Description,
                wi.State));

        var updateNotifications = _workerService.GetAll()
            .Where(wi => wi.UpdatedTimestamp == dateTimeOffset &&
                    _notifications.Any(n => n.State == wi.State))
            .Select(wi => new Notification(
                wi.Id,
                wi.UpdatedTimestamp,
                wi.Entity,
                wi.Description,
                wi.State));

        foreach (var notification in newNotifications)
        {
            var duplicateWorkItemIndex = _notifications.FindIndex(n => n.Id == notification.Id);
            if (duplicateWorkItemIndex > -1)
            {
                _notifications.RemoveAt(duplicateWorkItemIndex);
            }
        }

        _notifications.AddRange(newNotifications);
        _notifications.AddRange(updateNotifications);

        return newNotifications.Any();
    }

    public void Remove(Guid id)
    {
        var index = _notifications.FindIndex(n => n.Id == id);
        if (index > -1)
        {
            _notifications.RemoveAt(index);
        }
    }

    public ICollection<Notification> GetAll()
        => [.. _notifications.OrderByDescending(n => n.Timestamp)];
}
