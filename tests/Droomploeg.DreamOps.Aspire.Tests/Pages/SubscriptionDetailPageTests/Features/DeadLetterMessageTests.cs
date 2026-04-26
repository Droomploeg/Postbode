using Droomploeg.DreamOps.Aspire.Tests.Infrastructure;

namespace Droomploeg.DreamOps.Aspire.Tests.Pages.SubscriptionDetailPageTests.Features;

public class DeadLetterMessageTests : DreamOpsTestBase
{
    private const string TopicName = "test-topic-deadletter";
    private const string SubscriptionName = "sub-deadletter";

    // ===== Happy scenarios =====

    [Fact]
    public async Task DeadLetter_SingleMessage_Should_MoveToDeadLetterQueue()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendTopicMessageAsync(TopicName, "Move to DLQ");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekActiveAsync();
        Assert.Single(messages);

        await actor.DeadLetterAsync(messages.First(), "TestReason", "Test description");

        var activeCount = await Fixture.CountSubscriptionActiveMessagesAsync(TopicName, SubscriptionName);
        Assert.Equal(0, activeCount);

        var dlqMessages = await Fixture.PeekSubscriptionDeadLetterMessagesAsync(TopicName, SubscriptionName);
        Assert.Single(dlqMessages);
        Assert.Equal("Move to DLQ", dlqMessages[0].Body.ToString());
    }

    [Fact]
    public async Task DeadLetter_SingleMessage_Should_CloseOverlay()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendTopicMessageAsync(TopicName, "Close overlay");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekActiveAsync();

        await actor.DeadLetterAsync(messages.First(), "Reason", "Desc");

        Assert.False(actor.HasOpenOverlays);
    }

    [Fact]
    public async Task DeadLetter_FirstOfMultiple_Should_OnlyMoveOne()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendTopicMessageAsync(TopicName, "Msg 1");
        await Fixture.SendTopicMessageAsync(TopicName, "Msg 2");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekActiveAsync();
        Assert.Equal(2, messages.Count);

        await actor.DeadLetterAsync(messages.First(), "Reason", "Only first");

        Assert.Equal(1, await Fixture.CountSubscriptionActiveMessagesAsync(TopicName, SubscriptionName));
        Assert.Equal(1, await Fixture.CountSubscriptionDeadLetterMessagesAsync(TopicName, SubscriptionName));
    }

    // ===== Unhappy scenarios =====

    [Fact]
    public async Task DeadLetter_WithNoSelectedMessage_Should_NotMoveAnything()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendTopicMessageAsync(TopicName, "Should stay active");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        Assert.False(actor.HasOpenOverlays);

        var activeCount = await Fixture.CountSubscriptionActiveMessagesAsync(TopicName, SubscriptionName);
        Assert.Equal(1, activeCount);

        var dlqCount = await Fixture.CountSubscriptionDeadLetterMessagesAsync(TopicName, SubscriptionName);
        Assert.Equal(0, dlqCount);
    }

    // ===== Audit =====

    [Fact]
    public async Task DeadLetter_SingleMessage_Should_CreateAuditEntry()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendTopicMessageAsync(TopicName, "Audit DL");
        AuditLogger.Clear();

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekActiveAsync();

        await actor.DeadLetterAsync(messages.First(), "AuditReason", "Audit desc");

        var entries = AuditLogger.Entries.Where(e => e.Action == "DeadLetterMessageAsync").ToList();
        Assert.Single(entries);
        Assert.Contains("AuditReason", entries[0].Details!);
    }
}
