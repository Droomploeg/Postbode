using Droomploeg.DreamOps.Infrastructure.AzureServiceBus;
using Microsoft.AspNetCore.Components;
using Microsoft.Graph.Models;

namespace Droomploeg.DreamOps.WebApp.Components.Layout;

public partial class NavigationPath : ComponentBase
{
    private const string OverviewIcon = "path-icon-overview";
    private const string QueuesIcon = "path-icon-queues";
    private const string TopicsIcon = "path-icon-topics";
    private const string HomeIcon = "path-icon-home";

    private readonly List<KeyValue> _crumblePath = [new() { Key = "Home", Value = PageConstants.OverviewPage }];
    private string? _currentUrl;
    private string? _icon = OverviewIcon;


    internal void UpdatePath(string url, ServiceBusConnection connection)
    {
        _currentUrl = url;

        _crumblePath.Clear();

        if (_currentUrl.StartsWith(PageConstants.QueueOverviewPage) || _currentUrl.StartsWith(PageConstants.QueueDetailPage))
        {
            _crumblePath.Add(new() { Key = "Home", Value = PageConstants.HomePage });
            _crumblePath.Add(new() { Key = connection.Name, Value = PageConstants.OverviewPage });
            _crumblePath.Add(new() { Key = "Queues", Value = PageConstants.QueueOverviewPage });
            _icon = QueuesIcon;
        }
        else if (_currentUrl.StartsWith(PageConstants.SubscriptionOverviewPage) || _currentUrl.StartsWith(PageConstants.SubscriptionDetailPage))
        {
            _crumblePath.Add(new() { Key = "Home", Value = PageConstants.HomePage });
            _crumblePath.Add(new() { Key = connection.Name, Value = PageConstants.OverviewPage });
            _crumblePath.Add(new() { Key = "Subscriptions", Value = PageConstants.SubscriptionOverviewPage });
            _icon = TopicsIcon;
        }
        else if (_currentUrl.StartsWith(PageConstants.OverviewPage))
        {
            _crumblePath.Add(new() { Key = "Home", Value = PageConstants.HomePage });
            _crumblePath.Add(new() { Key = connection.Name, Value = PageConstants.OverviewPage });
            _icon = OverviewIcon;
        }
        else
        {
            _crumblePath.Add(new() { Key = "Home", Value = PageConstants.HomePage });
            _icon = HomeIcon;
        }

        StateHasChanged();
    }


}
