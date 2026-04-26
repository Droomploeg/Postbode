using Bunit;
using Droomploeg.DreamOps.WebApp.Components.Pages;

namespace Droomploeg.DreamOps.Aspire.Tests.Actors;

/// <summary>
/// Actor for interacting with the BackgroundJobsPage in tests.
/// Provides access to the worker items grid and actions.
/// </summary>
public class BackgroundJobsPageActor
{
    private readonly IRenderedComponent<BackgroundJobsPage> _page;

    private BackgroundJobsPageActor(IRenderedComponent<BackgroundJobsPage> page)
    {
        _page = page;
    }

    public bool ContainsText(string text) =>
        _page.Markup.Contains(text);

    public bool HasRefreshButton =>
        _page.FindAll(".refresh-button").Any();

    public bool HasWorkerItem(string description) =>
        _page.Markup.Contains(description);

    public bool HasState(string state) =>
        _page.Markup.Contains(state);

    public bool HasStopButton =>
        _page.FindAll(".stop-button").Any();

    public bool HasDeleteButton =>
        _page.FindAll(".delete-button").Any();

    public void ClickRefresh() =>
        _page.Find(".refresh-button").Click();

    public int WorkerItemCount =>
        _page.FindAll("gridRow").Count - 1; // minus header row

    public async Task ClickStopAsync()
    {
        var stopButton = _page.Find(".stop-button");
        await stopButton.ClickAsync(new());
    }

    public async Task ClickDeleteJobAsync()
    {
        var deleteButton = _page.Find(".delete-button");
        await deleteButton.ClickAsync(new());
    }

    public static BackgroundJobsPageActor Create(TestContext context)
    {
        var page = context.RenderComponent<BackgroundJobsPage>();
        return new BackgroundJobsPageActor(page);
    }
}
