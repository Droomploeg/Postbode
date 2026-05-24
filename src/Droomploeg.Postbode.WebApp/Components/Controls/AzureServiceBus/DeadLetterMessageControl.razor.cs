using Droomploeg.Postbode.WebApp.Components.Controls.AzureServiceBus.Models;
using Microsoft.AspNetCore.Components;

namespace Droomploeg.Postbode.WebApp.Components.Controls.AzureServiceBus;

public partial class DeadLetterMessageControl : ComponentBase
{
    [SupplyParameterFromForm] private DeadLetterMessageModel? Model { get; set; } 

    [Parameter] public EventCallback<DeadLetterMessageModel> OnSend { get; set; }

    [Parameter] public EventCallback OnCancel { get; set; }

    private string Reason
    {
        get => Model?.Reason ?? string.Empty;
        set
        {
            Model ??= new DeadLetterMessageModel();
            Model.Reason = value;
        } 
    }

    private string Description
    {
        get => Model?.Description ?? string.Empty;
        set
        {
            Model ??= new DeadLetterMessageModel();
            Model.Description = value;
        }
    }
    
    
    protected override void OnInitialized()
    {
        Model = new DeadLetterMessageModel();
        base.OnInitialized();
    }
    
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
