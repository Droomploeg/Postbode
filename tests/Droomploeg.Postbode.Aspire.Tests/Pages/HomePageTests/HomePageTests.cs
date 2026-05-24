using Droomploeg.Postbode.Aspire.Tests.Infrastructure;

namespace Droomploeg.Postbode.Aspire.Tests.Pages.HomePageTests;

public class HomePageTests : PostbodeTestBase
{
    [Fact]
    public void Should_RenderConnectionGrid()
    {
        var actor = HomePageActor.Create(this, "production", "staging");

        Assert.True(actor.HasConnection("production"));
        Assert.True(actor.HasConnection("staging"));
    }

    [Fact]
    public void SingleConnection_Should_AutoNavigateToOverview()
    {
        var actor = HomePageActor.Create(this, "my-servicebus");

        Assert.True(actor.HasNavigatedTo("/overview"));
    }

    [Fact]
    public void Should_RenderMultipleConnections()
    {
        var actor = HomePageActor.Create(this, "conn-a", "conn-b", "conn-c");

        Assert.True(actor.HasConnection("conn-a"));
        Assert.True(actor.HasConnection("conn-b"));
        Assert.True(actor.HasConnection("conn-c"));
    }
}
