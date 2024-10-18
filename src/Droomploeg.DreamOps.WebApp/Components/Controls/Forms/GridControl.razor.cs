using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.WebApp.Components.Controls.Forms;

public partial class GridControl<TItem> : ComponentBase
{
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public IEnumerable<TItem>? Items { get; set; }

    [Parameter] public EventCallback<TItem> OnSelected { get; set; }

    public TItem? SelectedItem { get; private set; }

    private List<GridHeaderItemControl<TItem>> HeaderItems { get; set; } = [];

    private List<GridValueItemControl<TItem>> ValueItems { get; set; } = [];

    public void AddHeaderItem(GridHeaderItemControl<TItem> item)
    {
        HeaderItems.Add(item);
    }

    public void AddValueItem(GridValueItemControl<TItem> item)
    {
        ValueItems.Add(item);
    }

    private async Task SelectRowClickAsync(TItem item)
    {
        SelectedItem = item;
        await OnSelected.InvokeAsync(item);
        StateHasChanged();
    }
}
