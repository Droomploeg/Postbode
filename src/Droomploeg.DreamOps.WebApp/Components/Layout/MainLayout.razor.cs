using Droomploeg.DreamOps.Domain.Workers.Models;
using Droomploeg.DreamOps.WebApp.Components.Controls.Workers.Models;
using Microsoft.AspNetCore.Components.Routing;

namespace Droomploeg.DreamOps.WebApp.Components.Layout;

public partial class MainLayout : IDisposable
{
    private readonly List<Notification> _popupNotifications = [];
    private readonly List<Notification> _listNotifications = [];
    private bool _notificationPanelVisible;
    private IDisposable? _locationChangeHandler;
    private Timer? _timer;

    private Menu? _menu;
    private NavigationPath? _navigationPath;

    protected override void OnInitialized()
    {
        _timer = new Timer(TimerElapsed, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _locationChangeHandler = _navigationManager.RegisterLocationChangingHandler(LocationChangingHandler);
            var relativeUrl = GetRelativeUri(_navigationManager.Uri);
            await (_menu?.UpdateAsync(relativeUrl) ?? Task.CompletedTask);
            await (_navigationPath?.UpdatePathAsync(relativeUrl) ?? Task.CompletedTask);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private ICollection<NotificationModel> PopupNotificationsToDisplay
        => [.. _popupNotifications
            .OrderByDescending(n => n.TimestampStateChanged)
            .Select(CastToNotificationModel)];
    
    private ICollection<NotificationModel> ListNotificationsToDisplay
        => [.. _listNotifications
            .OrderBy(n => n.TimestampCreated)
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

    private void ReturnToHome()
    {
        _navigationManager.NavigateTo(PageConstants.HomePage, true);
    }

    private void TimerElapsed(object? state)
    {
        var nowUtc = DateTimeOffset.UtcNow;

        var hasPopupUpdate = _notificationService.TryUpdatePopupNotifications(
            _popupNotifications,
            nowUtc, TimeSpan.FromSeconds(-5),
            out var updatedPopupNotifications);

        if (hasPopupUpdate)
        {
            _popupNotifications.Clear();
            _popupNotifications.AddRange(updatedPopupNotifications);
        }
        
        var listNotification = _notificationService.UpdateNotifications(
            _listNotifications,
            nowUtc);

        _listNotifications.Clear();
        _listNotifications.AddRange(listNotification);
       
        if (hasPopupUpdate || _notificationPanelVisible)
        {
            InvokeAsync(StateHasChanged);
        }
    }

    private static NotificationModel CastToNotificationModel(Notification notification)
        => new(
            notification.Id,
            notification.Entity,
            notification.Message,
            notification.State.ToString(),
            notification.Type,
            notification.TimestampCreated,
            notification.TimestampStateChanged);

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
        _listNotifications.RemoveAll(n => n.Id == id);
        StateHasChanged();
    }

    private string GetRelativeUri(string url)
    {
        return url.StartsWith(_navigationManager.BaseUri, StringComparison.CurrentCultureIgnoreCase)
            ? $"/{_navigationManager.ToBaseRelativePath(url)}"
            : url;
    }
}
