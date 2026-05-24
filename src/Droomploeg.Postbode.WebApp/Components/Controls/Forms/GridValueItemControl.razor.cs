using Microsoft.AspNetCore.Components;

namespace Droomploeg.Postbode.WebApp.Components.Controls.Forms;

public partial class GridValueItemControl<TItem> : ComponentBase
{
    protected override void OnInitialized()
    {
        base.OnInitialized();
        Parent.AddValueItem(this);
    }

    [CascadingParameter]
    private GridControl<TItem> Parent { get; set; } = default!;

    [Parameter]
    public RenderFragment<TItem> ChildContent { get; set; } = default!;

    internal TItem Value { get; set; } = default!;
}
