using Droomploeg.DreamOps.WebApp;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Graph.Models;

namespace Droomploeg.DreamOps.WebApp.Components.Layout;

public partial class NavigationPath : ComponentBase, IDisposable
{
    private const string OverviewIcon = "path-icon-overview";
    private const string QueuesIcon = "path-icon-queues";
    private const string TopicsIcon = "path-icon-topics";
    private const string HomeIcon = "path-icon-home";

    private readonly List<KeyValue> _crumblePath = [new() { Key = "Home", Value = PageConstants.OverviewPage }];
    private string? _currentUrl;
    private string? _icon = OverviewIcon;


    protected override void OnInitialized()
    {
        _currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        NavigationManager.LocationChanged += OnLocationChanged;

        UpdatePath();
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _currentUrl = NavigationManager.ToBaseRelativePath(e.Location);

        UpdatePath();
        StateHasChanged();
    }

    private void UpdatePath()
    {
        var relativeUrl = $"/{_currentUrl}";
        _crumblePath.Clear();

        if (relativeUrl.StartsWith(PageConstants.QueueOverviewPage) || relativeUrl.StartsWith(PageConstants.QueueDetailPage))
        {
            _crumblePath.Add(new() { Key = "Home", Value = PageConstants.HomePage });
            _crumblePath.Add(new() { Key = ServiceBusConnectionContext.Current.Name, Value = PageConstants.OverviewPage });
            _crumblePath.Add(new() { Key = "Queues", Value = PageConstants.QueueOverviewPage });
            _icon = QueuesIcon;
        }
        else if (relativeUrl.StartsWith(PageConstants.SubscriptionOverviewPage) || relativeUrl.StartsWith(PageConstants.SubscriptionDetailPage))
        {
            _crumblePath.Add(new() { Key = "Home", Value = PageConstants.HomePage });
            _crumblePath.Add(new() { Key = ServiceBusConnectionContext.Current.Name, Value = PageConstants.OverviewPage });
            _crumblePath.Add(new() { Key = "Subscriptions", Value = PageConstants.SubscriptionOverviewPage });
            _icon = TopicsIcon;
        }
        else if (relativeUrl.StartsWith(PageConstants.OverviewPage))
        {
            _crumblePath.Add(new() { Key = "Home", Value = PageConstants.HomePage });
            _crumblePath.Add(new() { Key = ServiceBusConnectionContext.Current.Name, Value = PageConstants.OverviewPage });
            _icon = OverviewIcon;
        }
        else
        {
            _crumblePath.Add(new() { Key = "Home", Value = PageConstants.HomePage });
            _icon = HomeIcon;
        }
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }

}
