using Droomploeg.DreamOps.Domain.Workers.Models;

namespace Droomploeg.DreamOps.WebApp.Components.Pages;

public partial class BackgroundJobsPage
{

    // TODO: Implement background job service page
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

        var items = WorkerService.GetAll();
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

        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        await item.CancelAsync(user.Identity?.Name ?? "Anonymous");
        _selectedWorker = null;

        UpdateWorkers();
    }

    private async Task DeleteWorkerAsync(WorkerItem item)
    {
        WorkerService.Remove(item.Id);

        UpdateWorkers();

        await Task.CompletedTask;
    }
}
