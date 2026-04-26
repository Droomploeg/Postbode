using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.Graph.Models;

namespace Droomploeg.DreamOps.WebApp.Components.Layout;

public partial class NavigationPath : ComponentBase
{
    private const string OverviewIcon = "path-icon-overview";
    private const string QueuesIcon = "path-icon-queues";
    private const string TopicsIcon = "path-icon-topics";
    private const string HomeIcon = "path-icon-home";
    private const string BackgroundIcon = "path-icon-backgroundjobs";

    private readonly List<KeyValue> _crumblePath = [new() { Key = "Home", Value = PageConstants.HomePage }];
    private string? _currentUrl;
    private string? _icon = OverviewIcon;
    private string _title = string.Empty;

    internal async Task UpdatePathAsync(string url)
    {
        _currentUrl = url;

        _crumblePath.Clear();

        var serviceBusConnection = await GetServiceBusConnection();

        if (serviceBusConnection.IsNotDefined)
        {
            _crumblePath.Add(new() { Key = "Home", Value = PageConstants.HomePage });
            _icon = HomeIcon;
            _title = "Home";
            StateHasChanged();
            return;
        }

        var connectionName = serviceBusConnection.Name;

        if (_currentUrl.StartsWith(PageConstants.BackgroundJobsPage))
        {
            _crumblePath.Add(new() { Key = "Home", Value = PageConstants.HomePage });
            _crumblePath.Add(new() { Key = connectionName, Value = PageConstants.OverviewPage });
            _icon = BackgroundIcon;
            _title = "Background jobs";
        }
        else if (_currentUrl.StartsWith(PageConstants.QueueOverviewPage) || _currentUrl.StartsWith(PageConstants.QueueDetailPage))
        {
            _crumblePath.Add(new() { Key = "Home", Value = PageConstants.HomePage });
            _crumblePath.Add(new() { Key = connectionName, Value = PageConstants.OverviewPage });
            _crumblePath.Add(new() { Key = "Queues", Value = PageConstants.QueueOverviewPage });
            _icon = QueuesIcon;
            _title = _currentUrl.StartsWith(PageConstants.QueueOverviewPage)
                ? "Queue-Overview"
                : $"Q{@_currentUrl?[2..]}";
        }
        else if (_currentUrl.StartsWith(PageConstants.SubscriptionOverviewPage) || _currentUrl.StartsWith(PageConstants.SubscriptionDetailPage))
        {
            _crumblePath.Add(new() { Key = "Home", Value = PageConstants.HomePage });
            _crumblePath.Add(new() { Key = connectionName, Value = PageConstants.OverviewPage });
            _crumblePath.Add(new() { Key = "Subscriptions", Value = PageConstants.SubscriptionOverviewPage });
            _icon = TopicsIcon;
            _title = _currentUrl.StartsWith(PageConstants.SubscriptionOverviewPage)
                ? "Topic-Overview"
                : $"T{@_currentUrl?[2..]}";
        }
        else if (_currentUrl.StartsWith(PageConstants.OverviewPage))
        {
            _crumblePath.Add(new() { Key = "Home", Value = PageConstants.HomePage });
            _crumblePath.Add(new() { Key = connectionName, Value = PageConstants.OverviewPage });
            _icon = OverviewIcon;
            _title = "Overview";
        }
        else
        {
            _crumblePath.Add(new() { Key = "Home", Value = PageConstants.HomePage });
            _icon = HomeIcon;
            _title = "Home";
        }

        StateHasChanged();
    }

    private async Task<ServiceBusConnection> GetServiceBusConnection()
    {
        var result = await Storage.GetAsync<ServiceBusConnection?>(nameof(ServiceBusConnection));
        if (!result.Success || result.Value is null || result.Value.Value == ServiceBusConnection.Undefined)
        {
            return ServiceBusConnection.Undefined;
        }

        return result.Value.Value;
    }
}
