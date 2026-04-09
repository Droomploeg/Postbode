using Bunit;
using Droomploeg.DreamOps.WebApp.Components.Pages;

namespace Droomploeg.DreamOps.Aspire.Tests.Actors;

/// <summary>
/// Actor for interacting with the OverviewPage (dashboard) in tests.
/// Provides access to queue and subscription summary counts and chart data.
/// </summary>
public class DashboardPageActor
{
    private readonly IRenderedComponent<OverviewPage> _page;

    private DashboardPageActor(IRenderedComponent<OverviewPage> page)
    {
        _page = page;
    }

    public bool ContainsText(string text) =>
        _page.Markup.Contains(text);

    public bool HasQueueSection =>
        _page.Markup.Contains("Queue");

    public bool HasSubscriptionSection =>
        _page.Markup.Contains("Subscriptions");

    public static DashboardPageActor Create(TestContext context)
    {
        var page = context.RenderComponent<OverviewPage>();
        page.WaitForState(() => !page.Markup.Contains("Loading"), TimeSpan.FromSeconds(5));
        return new DashboardPageActor(page);
    }
}
