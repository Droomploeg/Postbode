using Droomploeg.DreamOps.Infrastructure.HostedServices.WorkerService;
using Droomploeg.DreamOps.WebApp.Components.Controls.Forms.Models;

namespace Droomploeg.DreamOps.WebApp.Components.Layout;

public partial class MainLayout : IDisposable
{
    private readonly Dictionary<DateTimeOffset, IEnumerable<NotificationModel>> _notificationItems = [];
    private bool _notificationPanelVisible = false;
    private Timer? _timer;

    protected override void OnInitialized()
    {
        _timer = new Timer(UpdateNotification, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

        base.OnInitialized();
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }

    public void HandleException(Exception exception)
    {

    }

    public void UpdateNotification(object? state)
    {
        bool stateHasChanged = false;
        var items = Monitor.GetUpdatedWorkItems(DateTimeOffset.UtcNow.AddSeconds(-1));
        if (items.Any())
        {
            var ids = _notificationItems
                .SelectMany(x => x.Value)
                .Select(x => x.Id)
                .ToList();

            stateHasChanged = true;
            var values = items
                .Where(item => !ids.Contains(item.Id))
                .Select(CastToNotificationModel);

            _notificationItems.Add(DateTimeOffset.UtcNow, values);
        }

        var removeItems = _notificationItems.Where(x => x.Key < DateTimeOffset.UtcNow.AddSeconds(-3)).Select(x => x.Key).ToList();
        if (removeItems.Count > 0)
        {
            stateHasChanged = true;
            foreach (var key in removeItems)
            {
                _notificationItems.Remove(key);
            }
        }

        if (stateHasChanged)
        {
            InvokeAsync(() => StateHasChanged());
        }
    }

    private static NotificationModel CastToNotificationModel(WorkItem workItem)
        => new(workItem.Id, $"{workItem.Entity} - {workItem.Description}", GetNotificationType(workItem.State));

    private static NotificationType GetNotificationType(WorkItemState state)
    {
        return state switch
        {
            WorkItemState.Created => NotificationType.Information,
            WorkItemState.Processing => NotificationType.Information,
            WorkItemState.Completed => NotificationType.Completed,
            WorkItemState.Failed => NotificationType.Failure,
            WorkItemState.Cancelled => NotificationType.Information,
            _ => NotificationType.Information
        };
    }

    private void ToggleNotification()
    {
        _notificationPanelVisible = !_notificationPanelVisible;
        StateHasChanged();
    }

    private void CloseNotificationPanel(bool value)
    {
        _notificationPanelVisible = value;
    }

    private void RemoveNotificationItem(Guid id)
    {
        Monitor.Unregister(id);
        StateHasChanged();
    }

    private readonly TimeSpan timeSpan = TimeSpan.FromMinutes(1);
}
