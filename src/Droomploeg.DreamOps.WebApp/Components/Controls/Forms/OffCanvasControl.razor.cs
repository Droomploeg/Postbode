using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.WebApp.Components.Controls.Forms;

public partial class OffCanvasControl : ComponentBase
{
    private const string OffCanvasOpen = "open";
    private const string OffCanvasClosed = "closed";

    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public bool Visible { get; set; } 
    [Parameter] public string CssClass { get; set; } = string.Empty;

    private void ToggleOffCanvas()
    {
        InvokeAsync(() => OnClose.InvokeAsync(false));
    }

    private string GetOffCanvasClass()
    {
        return Visible ? OffCanvasOpen : OffCanvasClosed;
    }
}
