using Bunit;
using Droomploeg.DreamOps.WebApp.Components.Pages;
using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.Aspire.Tests.Actors;

public class OverviewPageActor<TPage> where TPage : IComponent
{
    private readonly IRenderedComponent<TPage> _page;

    public OverviewPageActor(IRenderedComponent<TPage> page)
    {
        _page = page;
        _page.WaitForState(() => !_page.Markup.Contains("Loading..."), TimeSpan.FromSeconds(5));
    }

    public bool ContainsText(string text) =>
        _page.Markup.Contains(text);

    public bool HasRefreshButton =>
        _page.FindAll(".refresh-button").Any();

    public bool HasLink(string text) =>
        _page.FindAll("a").Any(a => a.TextContent.Contains(text));

    public void ClickRefresh() =>
        _page.Find(".refresh-button").Click();
}

public static class OverviewPageActor
{
    public static OverviewPageActor<QueueOverviewPage> CreateQueueOverview(TestContext context)
    {
        var page = context.RenderComponent<QueueOverviewPage>();
        return new OverviewPageActor<QueueOverviewPage>(page);
    }

    public static OverviewPageActor<SubscriptionOverviewPage> CreateSubscriptionOverview(TestContext context)
    {
        var page = context.RenderComponent<SubscriptionOverviewPage>();
        return new OverviewPageActor<SubscriptionOverviewPage>(page);
    }
}
