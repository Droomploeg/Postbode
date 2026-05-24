using Droomploeg.Postbode.Aspire.Tests.Infrastructure;

namespace Droomploeg.Postbode.Aspire.Tests.Pages.SubscriptionOverviewPageTests;

public class SubscriptionOverviewPageTests : PostbodeTestBase
{
    [Fact]
    public void Should_RenderSubscriptionGrid()
    {
        var sub1 = RuntimeInfoService.CreateSubscription("topic-1", "sub-1", activeMessages: 5);
        var sub2 = RuntimeInfoService.CreateSubscription("topic-1", "sub-2", deadLetterMessages: 3);
        RuntimeInfoService.AddTopic("topic-1", sub1, sub2);

        var actor = OverviewPageActor.CreateSubscriptionOverview(this);

        Assert.True(actor.ContainsText("sub-1"));
        Assert.True(actor.ContainsText("sub-2"));
        Assert.True(actor.ContainsText("topic-1"));
    }

    [Fact]
    public void Should_ShowActiveMessageCount()
    {
        var sub = RuntimeInfoService.CreateSubscription("topic-active", "sub-active", activeMessages: 42);
        RuntimeInfoService.AddTopic("topic-active", sub);

        var actor = OverviewPageActor.CreateSubscriptionOverview(this);

        Assert.True(actor.ContainsText("42"));
    }

    [Fact]
    public void Should_ShowDeadLetterMessageCount()
    {
        var sub = RuntimeInfoService.CreateSubscription("topic-dlq", "sub-dlq", deadLetterMessages: 7);
        RuntimeInfoService.AddTopic("topic-dlq", sub);

        var actor = OverviewPageActor.CreateSubscriptionOverview(this);

        Assert.True(actor.ContainsText("7"));
    }

    [Fact]
    public void Should_ShowScheduledMessageCount()
    {
        var sub = RuntimeInfoService.CreateSubscription("topic-scheduled", "sub-scheduled", scheduledMessages: 15);
        RuntimeInfoService.AddTopic("topic-scheduled", sub);

        var actor = OverviewPageActor.CreateSubscriptionOverview(this);

        Assert.True(actor.ContainsText("15"));
    }

    [Fact]
    public void Should_RenderRefreshButton()
    {
        var sub = RuntimeInfoService.CreateSubscription("topic-1", "sub-1");
        RuntimeInfoService.AddTopic("topic-1", sub);

        var actor = OverviewPageActor.CreateSubscriptionOverview(this);

        Assert.True(actor.HasRefreshButton);
    }

    [Fact]
    public void Should_RenderSubscriptionNameAsLink()
    {
        var sub = RuntimeInfoService.CreateSubscription("my-topic", "my-subscription");
        RuntimeInfoService.AddTopic("my-topic", sub);

        var actor = OverviewPageActor.CreateSubscriptionOverview(this);

        Assert.True(actor.HasLink("my-subscription"));
    }

    [Fact]
    public void Should_ShowTopicName()
    {
        var sub = RuntimeInfoService.CreateSubscription("orders-topic", "order-sub");
        RuntimeInfoService.AddTopic("orders-topic", sub);

        var actor = OverviewPageActor.CreateSubscriptionOverview(this);

        Assert.True(actor.ContainsText("orders-topic"));
    }

    [Fact]
    public void Should_ShowSubscriptionsFromMultipleTopics()
    {
        var sub1 = RuntimeInfoService.CreateSubscription("topic-a", "sub-a");
        var sub2 = RuntimeInfoService.CreateSubscription("topic-b", "sub-b");
        RuntimeInfoService.AddTopic("topic-a", sub1);
        RuntimeInfoService.AddTopic("topic-b", sub2);

        var actor = OverviewPageActor.CreateSubscriptionOverview(this);

        Assert.True(actor.ContainsText("sub-a"));
        Assert.True(actor.ContainsText("sub-b"));
        Assert.True(actor.ContainsText("topic-a"));
        Assert.True(actor.ContainsText("topic-b"));
    }

    [Fact]
    public void Refresh_Should_UpdateGrid()
    {
        var sub = RuntimeInfoService.CreateSubscription("topic-refresh", "sub-refresh");
        RuntimeInfoService.AddTopic("topic-refresh", sub);

        var actor = OverviewPageActor.CreateSubscriptionOverview(this);
        Assert.True(actor.ContainsText("sub-refresh"));

        actor.ClickRefresh();

        Assert.True(actor.ContainsText("sub-refresh"));
    }

    [Fact]
    public void EmptyGrid_Should_NotShowSubscriptionNames()
    {
        var actor = OverviewPageActor.CreateSubscriptionOverview(this);

        Assert.False(actor.HasLink("sub-"));
    }
}
