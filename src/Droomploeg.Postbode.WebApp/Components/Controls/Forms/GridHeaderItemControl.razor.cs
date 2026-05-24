using Microsoft.AspNetCore.Components;

namespace Droomploeg.Postbode.WebApp.Components.Controls.Forms;

public partial class GridHeaderItemControl<TItem> : ComponentBase
{
    protected override void OnInitialized()
    {
        base.OnInitialized();
        Parent.AddHeaderItem(this);
    }


    [CascadingParameter] private GridControl<TItem> Parent { get; set; } = default!;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string CssClass { get; set; } = string.Empty;

}
