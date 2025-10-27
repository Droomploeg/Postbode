using AngleSharp.Dom;
using Bunit;
using Droomploeg.DreamOps.Domain.ServiceBus;
using Droomploeg.DreamOps.IntegrationsTests.Common;
using Droomploeg.DreamOps.IntegrationsTests.Pages.Queues.GridColumns;
using Droomploeg.DreamOps.WebApp.Components.Controls.Forms;
using Droomploeg.DreamOps.WebApp.Components.Pages;

namespace Droomploeg.DreamOps.IntegrationsTests.Pages.Queues.Extensions;

internal static class QueueOverviewExtension
{
    internal static IRenderedComponent<GridControl<Queue>> GetGrid(this IRenderedComponent<QueueOverviewPage> page)
    {
        return page.FindComponent<GridControl<Queue>>();
    }

    internal static long GetColumnValueOrRow(this IRenderedComponent<QueueOverviewPage> page, string queueName, QueueOverviewGridColumn column)
    {
        var body = page.Find(HtmlTags.TableBody);
        var rows = body?.GetElementsByTagName(HtmlTags.Row)!;
        var row = rows.FirstOrDefault(r => r.GetElementsByTagName(HtmlTags.Link)?[0].TextContent == queueName);

        return long.Parse(row!.GetElementsByTagName(HtmlTags.Column)[(int)column].TextContent);
    }
}
