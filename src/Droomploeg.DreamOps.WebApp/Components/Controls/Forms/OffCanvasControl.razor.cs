using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.WebApp.Components.Controls.Forms;

public partial class OffCanvasControl : ComponentBase
{
    private const string OffCanvasOpen = "open";
    private const string OffCanvasClosed = "closed";

    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback<bool> OnClose { get; set; }
    [Parameter] public bool Visible { get; set; } = false;
    [Parameter] public string CssClass { get; set; } = string.Empty;

    public void ToggleOffCanvas()
    {
        InvokeAsync(() => OnClose.InvokeAsync(!Visible));
    }

    private string GetOffCanvasClass()
    {
        return Visible ? OffCanvasOpen : OffCanvasClosed;
    }
}
