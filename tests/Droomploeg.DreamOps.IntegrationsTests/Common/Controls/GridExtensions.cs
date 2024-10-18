using Droomploeg.DreamOps.WebApp.Components.Controls.Forms;

namespace Droomploeg.DreamOps.IntegrationsTests.Common.Controls;

internal static class GridExtensions
{
    internal static bool IsLoading<TItem>(this GridControl<TItem> grid)
    {
        return grid.Items == null;
    }
}
