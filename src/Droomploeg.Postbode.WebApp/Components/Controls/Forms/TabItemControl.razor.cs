using Microsoft.AspNetCore.Components;

namespace Droomploeg.Postbode.WebApp.Components.Controls.Forms;

public partial class TabItemControl : ComponentBase
{
    protected override void OnInitialized()
    {
        base.OnInitialized();
        Parent.AddTabItem(this);
    }

    [CascadingParameter]
    private TabControl Parent { get; set; } = null!;

    [Parameter]
    public string? Id { get; set; } = null;

    [Parameter]
    public object? Tag { get; set; } = null;

    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    public bool IsSelected { get; private set; } = false;

    public void Deselect()
    {
        IsSelected = false;
    }

    public void Select()
    {
        IsSelected = true;
    }
}


