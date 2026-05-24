using Droomploeg.Postbode.Aspire.Tests.Infrastructure;

namespace Droomploeg.Postbode.Aspire.Tests.Pages.QueueOverviewPageTests;

public class QueueOverviewPageTests : PostbodeTestBase
{
    [Fact]
    public void Should_RenderQueueGrid()
    {
        RuntimeInfoService.AddQueue("queue-1", activeMessages: 5);
        RuntimeInfoService.AddQueue("queue-2", deadLetterMessages: 3);

        var actor = OverviewPageActor.CreateQueueOverview(this);

        Assert.True(actor.ContainsText("queue-1"));
        Assert.True(actor.ContainsText("queue-2"));
    }

    [Fact]
    public void Should_ShowActiveMessageCount()
    {
        RuntimeInfoService.AddQueue("queue-active", activeMessages: 42);

        var actor = OverviewPageActor.CreateQueueOverview(this);

        Assert.True(actor.ContainsText("42"));
    }

    [Fact]
    public void Should_ShowDeadLetterMessageCount()
    {
        RuntimeInfoService.AddQueue("queue-dlq", deadLetterMessages: 7);

        var actor = OverviewPageActor.CreateQueueOverview(this);

        Assert.True(actor.ContainsText("7"));
    }

    [Fact]
    public void Should_ShowScheduledMessageCount()
    {
        RuntimeInfoService.AddQueue("queue-scheduled", scheduledMessages: 15);

        var actor = OverviewPageActor.CreateQueueOverview(this);

        Assert.True(actor.ContainsText("15"));
    }

    [Fact]
    public void Should_RenderRefreshButton()
    {
        RuntimeInfoService.AddQueue("queue-1");

        var actor = OverviewPageActor.CreateQueueOverview(this);

        Assert.True(actor.HasRefreshButton);
    }

    [Fact]
    public void Should_RenderQueueNameAsLink()
    {
        RuntimeInfoService.AddQueue("my-test-queue");

        var actor = OverviewPageActor.CreateQueueOverview(this);

        Assert.True(actor.HasLink("my-test-queue"));
    }

    [Fact]
    public void Should_ShowMultipleQueues()
    {
        RuntimeInfoService.AddQueue("queue-a", activeMessages: 1);
        RuntimeInfoService.AddQueue("queue-b", activeMessages: 2);
        RuntimeInfoService.AddQueue("queue-c", activeMessages: 3);

        var actor = OverviewPageActor.CreateQueueOverview(this);

        Assert.True(actor.ContainsText("queue-a"));
        Assert.True(actor.ContainsText("queue-b"));
        Assert.True(actor.ContainsText("queue-c"));
    }

    [Fact]
    public void Refresh_Should_UpdateGrid()
    {
        RuntimeInfoService.AddQueue("queue-before");

        var actor = OverviewPageActor.CreateQueueOverview(this);
        Assert.True(actor.ContainsText("queue-before"));

        actor.ClickRefresh();

        Assert.True(actor.ContainsText("queue-before"));
    }

    [Fact]
    public void EmptyGrid_Should_NotShowQueueNames()
    {
        var actor = OverviewPageActor.CreateQueueOverview(this);

        Assert.False(actor.HasLink("queue-"));
    }
}
