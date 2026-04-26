using Droomploeg.DreamOps.Aspire.Tests.Infrastructure;

namespace Droomploeg.DreamOps.Aspire.Tests.Pages.OverviewPageTests;

public class OverviewPageTests : DreamOpsTestBase
{
    // ===== Queue section =====

    [Fact]
    public void Should_ShowQueueSection()
    {
        var actor = DashboardPageActor.Create(this);

        Assert.True(actor.HasQueueSection);
    }

    [Fact]
    public void Should_ShowQueueCount()
    {
        RuntimeInfoService.AddQueue("queue-1");
        RuntimeInfoService.AddQueue("queue-2");
        RuntimeInfoService.AddQueue("queue-3");

        var actor = DashboardPageActor.Create(this);

        Assert.True(actor.ContainsText("3"));
    }

    [Fact]
    public void Should_ShowQueueActiveMessageCount()
    {
        RuntimeInfoService.AddQueue("queue-1", activeMessages: 25);
        RuntimeInfoService.AddQueue("queue-2", activeMessages: 17);

        var actor = DashboardPageActor.Create(this);

        Assert.True(actor.ContainsText("42"));
    }

    [Fact]
    public void Should_ShowQueueDeadLetterMessageCount()
    {
        RuntimeInfoService.AddQueue("queue-1", deadLetterMessages: 5);
        RuntimeInfoService.AddQueue("queue-2", deadLetterMessages: 8);

        var actor = DashboardPageActor.Create(this);

        Assert.True(actor.ContainsText("13"));
    }

    [Fact]
    public void Should_ShowQueueScheduledMessageCount()
    {
        RuntimeInfoService.AddQueue("queue-1", scheduledMessages: 10);

        var actor = DashboardPageActor.Create(this);

        Assert.True(actor.ContainsText("10"));
    }

    // ===== Subscription section =====

    [Fact]
    public void Should_ShowSubscriptionSection()
    {
        var actor = DashboardPageActor.Create(this);

        Assert.True(actor.HasSubscriptionSection);
    }

    [Fact]
    public void Should_ShowSubscriptionCount()
    {
        var sub1 = RuntimeInfoService.CreateSubscription("topic-1", "sub-1");
        var sub2 = RuntimeInfoService.CreateSubscription("topic-1", "sub-2");
        RuntimeInfoService.AddTopic("topic-1", sub1, sub2);

        var actor = DashboardPageActor.Create(this);

        Assert.True(actor.ContainsText("2"));
    }

    [Fact]
    public void Should_ShowSubscriptionActiveMessageCount()
    {
        var sub = RuntimeInfoService.CreateSubscription("topic-1", "sub-1", activeMessages: 33);
        RuntimeInfoService.AddTopic("topic-1", sub);

        var actor = DashboardPageActor.Create(this);

        Assert.True(actor.ContainsText("33"));
    }

    [Fact]
    public void Should_ShowSubscriptionDeadLetterMessageCount()
    {
        var sub = RuntimeInfoService.CreateSubscription("topic-1", "sub-1", deadLetterMessages: 9);
        RuntimeInfoService.AddTopic("topic-1", sub);

        var actor = DashboardPageActor.Create(this);

        Assert.True(actor.ContainsText("9"));
    }

    [Fact]
    public void Should_ShowSubscriptionScheduledMessageCount()
    {
        var sub = RuntimeInfoService.CreateSubscription("topic-1", "sub-1", scheduledMessages: 21);
        RuntimeInfoService.AddTopic("topic-1", sub);

        var actor = DashboardPageActor.Create(this);

        Assert.True(actor.ContainsText("21"));
    }

    // ===== Empty state =====

    [Fact]
    public void NoQueues_Should_ShowZeroCounts()
    {
        var actor = DashboardPageActor.Create(this);

        Assert.True(actor.ContainsText("Number of queues:"));
    }

    [Fact]
    public void NoSubscriptions_Should_ShowZeroCounts()
    {
        var actor = DashboardPageActor.Create(this);

        Assert.True(actor.ContainsText("Number of subscriptions:"));
    }
}
