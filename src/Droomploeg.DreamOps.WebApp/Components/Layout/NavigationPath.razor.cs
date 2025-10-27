using Droomploeg.DreamOps.Domain.ServiceBus.Models;
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

    private readonly List<KeyValue> _crumblePath = [new() { Key = "Home", Value = PageConstants.HomePage }];
    private string? _currentUrl;
    private string? _icon = OverviewIcon;

    internal async Task UpdatePathAsync(string url)
    {
        _currentUrl = url;

        _crumblePath.Clear();

        var serviceBusConnectionInfo = await GetServiceBusConnectionInfo();

        if (serviceBusConnectionInfo is null)
        {
            _crumblePath.Add(new() { Key = "Home", Value = PageConstants.HomePage });
            _icon = HomeIcon;
            StateHasChanged();
            return;
        }

        var connectionName = serviceBusConnectionInfo.Connection.Name;

        if (_currentUrl.StartsWith(PageConstants.QueueOverviewPage) || _currentUrl.StartsWith(PageConstants.QueueDetailPage))
        {
            _crumblePath.Add(new() { Key = "Home", Value = PageConstants.HomePage });
            _crumblePath.Add(new() { Key = connectionName, Value = PageConstants.OverviewPage });
            _crumblePath.Add(new() { Key = "Queues", Value = PageConstants.QueueOverviewPage });
            _icon = QueuesIcon;
        }
        else if (_currentUrl.StartsWith(PageConstants.SubscriptionOverviewPage) || _currentUrl.StartsWith(PageConstants.SubscriptionDetailPage))
        {
            _crumblePath.Add(new() { Key = "Home", Value = PageConstants.HomePage });
            _crumblePath.Add(new() { Key = connectionName, Value = PageConstants.OverviewPage });
            _crumblePath.Add(new() { Key = "Subscriptions", Value = PageConstants.SubscriptionOverviewPage });
            _icon = TopicsIcon;
        }
        else if (_currentUrl.StartsWith(PageConstants.OverviewPage))
        {
            _crumblePath.Add(new() { Key = "Home", Value = PageConstants.HomePage });
            _crumblePath.Add(new() { Key = connectionName, Value = PageConstants.OverviewPage });
            _icon = OverviewIcon;
        }
        else
        {
            _crumblePath.Add(new() { Key = "Home", Value = PageConstants.HomePage });
            _icon = HomeIcon;
        }

        StateHasChanged();
    }

    private async Task<ServiceBusConnectionInfo?> GetServiceBusConnectionInfo()
    {
        var result = await _storage.GetAsync<ServiceBusConnectionInfo>(nameof(ServiceBusConnectionInfo));
        if (!result.Success || result.Value is null || result.Value?.Connection == ServiceBusConnection.Undefined)
        {
            return null;
        }

        return result.Value;
    }
}
