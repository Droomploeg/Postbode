using Droomploeg.DreamOps.Core.Models;

namespace Droomploeg.DreamOps.WebApp.Components.Pages;

public partial class SubscriptionOverviewPage
{
    private List<Subscription>? _subscriptions = null;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await UpdateEntitiesAsync();
            StateHasChanged();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task RefreshAsync()
    {
        await UpdateEntitiesAsync();
        StateHasChanged();
    }

    private async Task UpdateEntitiesAsync()
    {
        var entities = await ServiceBusService.GetAllTopicsAsync();
        _subscriptions = new List<Subscription>(entities.SelectMany(t => t.Subscriptions));
    }

    private static string GetLink(Subscription subscription)
        => $"{PageConstants.SubscriptionDetailPage}/{Uri.EscapeDataString(subscription.TopicName)}/{Uri.EscapeDataString(subscription.Name)}";
}
