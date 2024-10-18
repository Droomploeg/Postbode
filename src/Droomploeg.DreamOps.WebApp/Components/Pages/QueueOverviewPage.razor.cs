using Droomploeg.DreamOps.Core.Models;
using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.WebApp.Components.Pages;

public partial class QueueOverviewPage
{
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
        var entities = await ServiceBusService.GetAllQueuesAsync();
        _queues = new List<Queue>(entities);
    }

    private static string GetLink(Queue queue)
        => $"{PageConstants.QueueDetailPage}/{Uri.EscapeDataString(queue.Name)}";
}
