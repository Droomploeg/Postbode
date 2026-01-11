using Droomploeg.DreamOps.WebApp.Components.Controls.Workers.Models;
using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.WebApp.Components.Controls.Workers;
public partial class NotificationControl : ComponentBase
{
    [Parameter] public NotificationModel Model { get; set; } = null!;
    [Parameter] public EventCallback OnClose { get; set; }

    private bool IsVisible { get; set; } = true;

    private string Message => Model.Message;
    
    private string Entity => Model.Entity;

    private string State => Model.State;

    private string NotificationClass => NotificationIconHelper.GetIcon(Model.Type);

    private void Close()
    {
        IsVisible = false;
        OnClose.InvokeAsync();
        InvokeAsync(() => StateHasChanged());
    }
}
