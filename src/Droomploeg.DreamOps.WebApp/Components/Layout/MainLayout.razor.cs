using Droomploeg.DreamOps.Domain.Workers.Models;
using Droomploeg.DreamOps.WebApp.Components.Controls.Workers.Models;
using Microsoft.AspNetCore.Components.Routing;

namespace Droomploeg.DreamOps.WebApp.Components.Layout;

public partial class MainLayout : IDisposable
{
    private List<Notification> _popupNotifications = [];
    private List<NotificationModel> _notifications = [];
    private bool _notificationPanelVisible = false;
    private IDisposable? _locationChangeHandler;
    private Timer? _timer;

    private Menu? _menu = default;
    private NavigationPath? _navigationPath = default;

    protected override void OnInitialized()
    {
        _timer = new Timer(TimerElapsed, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _locationChangeHandler = _navigationManager.RegisterLocationChangingHandler(LocationChangingHandler);
            var relativeUrl = GetRelativeUri(_navigationManager.Uri.ToString());
            await (_menu?.UpdateAsync(relativeUrl) ?? Task.CompletedTask);
            await (_navigationPath?.UpdatePathAsync(relativeUrl) ?? Task.CompletedTask);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private ICollection<NotificationModel> PopupNotificationsToDisplay
        => [.. _popupNotifications
            .OrderByDescending(n => n.Timestamp)
            .Select(CastToNotificationModel)];
    
    private async ValueTask LocationChangingHandler(LocationChangingContext arg)
    {
        var relativeUrl = GetRelativeUri(arg.TargetLocation);
        await (_menu?.UpdateAsync(relativeUrl) ?? Task.CompletedTask);
        await (_navigationPath?.UpdatePathAsync(relativeUrl) ?? Task.CompletedTask);
    }

    public void Dispose()
    {
        _locationChangeHandler?.Dispose();
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }

    public void ReturnToHome()
    {
        _navigationManager.NavigateTo(PageConstants.HomePage, true);
    }

    public void TimerElapsed(object? state)
    {
        var nowUtc = DateTimeOffset.UtcNow;

        var hasPopupUpdate = _notificationService.TryUpdatePopupNotifications(
            _popupNotifications,
            nowUtc, TimeSpan.FromSeconds(-3),
            out var updatedPopupNotifications);

        if (hasPopupUpdate)
        {
            _popupNotifications = [.. updatedPopupNotifications];
        }

        //var stateHasChanged = _notificationService.CleanUp(nowUtc.AddMinutes(-3)) ||
        //if (stateHasChanged)
        //{
        //    _notifications = [.._notificationService.GetAll()
        //        .Select(CastToNotificationModel)];
        //}

        if (true)
        {
            InvokeAsync(() => StateHasChanged());
        }
    }

    private static NotificationModel CastToNotificationModel(Notification notification)
        => new(
            notification.Id,
            notification.Entity,
            notification.Message,
            notification.Type,
            notification.Timestamp);

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
        // todo: notification remove
        //Monitor.Unregister(id);
        StateHasChanged();
    }

    private string GetRelativeUri(string url)
    {
        return url.StartsWith(_navigationManager.BaseUri, StringComparison.CurrentCultureIgnoreCase)
            ? $"/{_navigationManager.ToBaseRelativePath(url)}"
            : url;
    }
}
