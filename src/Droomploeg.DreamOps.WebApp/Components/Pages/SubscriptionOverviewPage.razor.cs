using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Droomploeg.DreamOps.WebApp.Components.Controls.Security;
using Microsoft.Identity.Web;

namespace Droomploeg.DreamOps.WebApp.Components.Pages;

public partial class SubscriptionOverviewPage
{
    private List<Subscription>? _subscriptions = null;
    private AuthorizationState _authorizationState = AuthorizationState.Authorized;

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
        try
        {
            var entities = await RuntimeInfoService.GetAllTopicsAsync();
            _subscriptions = [.. entities.SelectMany(t => t.Subscriptions)];
            _authorizationState = AuthorizationState.Authorized;
        }
        catch (UnauthorizedAccessException)
        {
            _authorizationState = AuthorizationState.Unauthorized;
        }
        catch (MicrosoftIdentityWebChallengeUserException)
        {
            _authorizationState = AuthorizationState.TokenExpired;
        }
    }

    private static string GetLink(Subscription subscription)
        => $"{PageConstants.SubscriptionDetailPage}/{Uri.EscapeDataString(subscription.TopicName)}/{Uri.EscapeDataString(subscription.Name)}";
}
