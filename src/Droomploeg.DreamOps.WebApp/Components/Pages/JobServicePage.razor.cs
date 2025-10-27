using Droomploeg.DreamOps.Domain.Workers.Models;

namespace Droomploeg.DreamOps.WebApp.Components.Pages;

public partial class JobServicePage
{

    // TODO: Implement background job service page
    // TODO: Queue/Subscription background service action disable when not enabled
    // TODO: Background service running with managed identity
    // TODO: Audit log for all actions incl. background job actions

    private readonly List<WorkerItem> _items = [];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _items.Clear();

            //try
            //{
            var items = _workerService.GetAll();

            _items.AddRange(items);
            //    _authorizationState = AuthorizationState.Authorized;
            //}
            //catch (UnauthorizedAccessException)
            //{
            //    _authorizationState = AuthorizationState.Unauthorized;
            //}
            //catch (MicrosoftIdentityWebChallengeUserException)
            //{
            //    _authorizationState = AuthorizationState.TokenExpired;
            //}

            StateHasChanged();
        }

        await base.OnAfterRenderAsync(firstRender);
    }
}
