using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.WebApp.Components.Controls.Forms;

public partial class TabControl : ComponentBase
{
    private List<TabItemControl> TabItems { get; set; } = [];

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public EventCallback<TabItemControl> TabChanged { get; set; }

    public TabItemControl Selected => TabItems.Single(x => x.IsSelected);

    public void AddTabItem(TabItemControl item)
    {
        TabItems.Add(item);

        var selected = TabItems.FirstOrDefault(x => x.IsSelected);
        if (selected is null)
        {
            item.Select();
        }

        StateHasChanged();
    }

    public async Task SelectedItemAsync(string name)
    {
        var item = TabItems.FirstOrDefault(x => x.Title == name);
        if (item is null)
            return;

        await SelectTabAsync(item);
    }

    private async Task SelectTabAsync(TabItemControl selectedTab)
    {
        foreach (var tabItem in TabItems)
        {
            tabItem.Deselect();
        }
        selectedTab.Select();
        await TabChanged.InvokeAsync(selectedTab);
        StateHasChanged();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (TabItems.Count > 0)
        {
            TabItems[0]!.Select();
        }
    }
}
