using Droomploeg.DreamOps.WebApp.Components.Controls.AzureServiceBus.Models;
using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.WebApp.Components.Controls.AzureServiceBus;

public partial class DeadLetterMessageControl : ComponentBase
{
    [SupplyParameterFromForm] private DeadLetterMessageModel Model { get; set; } = new DeadLetterMessageModel();

    [Parameter] public EventCallback<DeadLetterMessageModel> OnSend { get; set; }

    [Parameter] public EventCallback OnCancel { get; set; }

    private async Task SendAsync()
    {
        if (OnSend.HasDelegate)
        {
            await OnSend.InvokeAsync(Model);
        }

        Model = new DeadLetterMessageModel();
        StateHasChanged();
    }

    private async Task CancelAsync()
    {
        if (OnCancel.HasDelegate)
        {
            await OnCancel.InvokeAsync();
        }
    }
}
