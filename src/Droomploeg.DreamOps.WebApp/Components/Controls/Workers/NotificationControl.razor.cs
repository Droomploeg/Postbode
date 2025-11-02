using Droomploeg.DreamOps.WebApp.Components.Controls.Workers.Models;
using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.WebApp.Components.Controls.Workers;
public partial class NotificationControl : ComponentBase
{
    [Parameter] public NotificationModel Model { get; set; } = null!;
    [Parameter] public EventCallback OnClose { get; set; }

    public bool IsVisible { get; set; } = true;

    public string Message => Model.Message;

    public string NotificationClass => NotificationIconHelper.GetIcon(Model.Type);

    public void Close()
    {
        IsVisible = false;
        OnClose.InvokeAsync();
        InvokeAsync(() => StateHasChanged());
    }
}
