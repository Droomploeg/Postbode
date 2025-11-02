using Droomploeg.DreamOps.Domain.Workers.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace Droomploeg.DreamOps.WebApp.Components.Pages;

public partial class JobServicePage
{

    // TODO: Implement background job service page
    // TODO: Queue/Subscription background service action disable when not enabled
    // TODO: Background service running with managed identity
    // TODO: Audit log for all actions incl. background job actions

    private readonly List<WorkerItem> _items = [];
    private WorkerItem? _selectedWorker;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            UpdateWorkers();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private void Refresh()
    {
        UpdateWorkers();
    }

    private void OnSelected(WorkerItem item)
    {
        _selectedWorker = item;

        StateHasChanged();
    }

    private void UpdateWorkers()
    {
        _items.Clear();

        var items = _workerService.GetAll();
        _items.AddRange(items);
        _selectedWorker = null;

        StateHasChanged();
    }

    private async Task StopWorkerAsync(WorkerItem item)
    {
        if (item == null)
        {
            return;
        }

        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        await item.CancelAsync(user.Identity?.Name ?? "Anonymous");
        _selectedWorker = null;

        UpdateWorkers();
    }

    private async Task DeleteWorkerAsync(WorkerItem item)
    {
        _workerService.Remove(item.Id);

        UpdateWorkers();

        await Task.CompletedTask;
    }
}
