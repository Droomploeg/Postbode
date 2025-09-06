using Droomploeg.DreamOps.Core.Models;
using Droomploeg.DreamOps.WebApp.Components.Controls.Security;
using Microsoft.Identity.Web;

namespace Droomploeg.DreamOps.WebApp.Components.Pages;

public partial class QueueOverviewPage
{
    private AuthorizationState _authorizationState = AuthorizationState.Authorized;
    private List<Queue>? _queues = null;

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
            var entities = await ServiceBusService.GetAllQueuesAsync();
            _queues = new List<Queue>(entities);
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

    private static string GetLink(Queue queue)
        => $"{PageConstants.QueueDetailPage}/{Uri.EscapeDataString(queue.Name)}";
}
