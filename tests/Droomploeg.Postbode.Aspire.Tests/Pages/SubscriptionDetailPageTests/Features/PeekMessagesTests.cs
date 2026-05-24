using Droomploeg.Postbode.Aspire.Tests.Infrastructure;

namespace Droomploeg.Postbode.Aspire.Tests.Pages.SubscriptionDetailPageTests.Features;

public class PeekMessagesTests : PostbodeTestBase
{
    private const string TopicName = "test-topic-peek";
    private const string SubscriptionName = "sub-peek";

    // ===== Happy scenarios - Active =====

    [Fact]
    public async Task PeekActive_WithMessages_Should_ReturnMessages()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendTopicMessageAsync(TopicName, "Message 1");
        await Fixture.SendTopicMessageAsync(TopicName, "Message 2");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekActiveAsync();

        Assert.Equal(2, messages.Count);
    }

    [Fact]
    public async Task PeekActive_EmptySubscription_Should_ReturnEmpty()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekActiveAsync();

        Assert.Empty(messages);
    }

    [Fact]
    public async Task PeekActive_Should_ReturnMessageContent()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendTopicMessageAsync(TopicName, "Expected topic body");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekActiveAsync();

        Assert.Single(messages);
        Assert.Equal("Expected topic body", messages.First().Body.ToString());
    }

    [Fact]
    public async Task PeekActive_WithLimit_Should_RespectNumberOfMessages()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        for (var i = 0; i < 5; i++)
        {
            await Fixture.SendTopicMessageAsync(TopicName, $"Message {i}");
        }

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        var messages = await actor.PeekActiveAsync(numberOfMessages: 2);

        Assert.Equal(2, messages.Count);
    }

    // ===== Happy scenarios - Dead-letter =====
    // NOTE: TopicService.PeekDeadLetterMessagesAsync currently uses _activeTopicAdapterFactory
    // instead of _deadLetterAdapterFactory. This means DLQ peek goes to the active subscription
    // path. These tests are skipped until that bug is fixed.

    [Fact]
    public async Task PeekDeadLetter_WithMessages_Should_ReturnMessages()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendAndDeadLetterSubscriptionMessageAsync(TopicName, SubscriptionName, "DLQ 1");
        await Fixture.SendAndDeadLetterSubscriptionMessageAsync(TopicName, SubscriptionName, "DLQ 2");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekDeadLetterAsync();

        Assert.Equal(2, messages.Count);
    }

    [Fact]
    public async Task PeekDeadLetter_EmptySubscription_Should_ReturnEmpty()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekDeadLetterAsync();

        Assert.Empty(messages);
    }

    [Fact]
    public async Task PeekDeadLetter_Should_ReturnMessageContent()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendAndDeadLetterSubscriptionMessageAsync(TopicName, SubscriptionName, "Dead letter body");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekDeadLetterAsync();

        Assert.Single(messages);
        Assert.Equal("Dead letter body", messages.First().Body.ToString());
    }

    // ===== Unhappy scenarios =====

    [Fact]
    public async Task PeekActive_AfterAllMessagesDeleted_Should_ReturnEmpty()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendTopicMessageAsync(TopicName, "Temporary message");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekActiveAsync();
        Assert.Single(messages);

        // Delete the message outside the page
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);

        messages = await actor.PeekActiveAsync();

        Assert.Empty(messages);
    }

    // ===== Audit =====

    [Fact]
    public async Task PeekActive_Should_CreateAuditEntry()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendTopicMessageAsync(TopicName, "Audit peek");
        AuditLogger.Clear();

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        await actor.PeekActiveAsync();

        var entries = AuditLogger.Entries.Where(e => e.Action == "PeekActiveMessagesAsync").ToList();
        Assert.Single(entries);
    }

    [Fact]
    public async Task PeekDeadLetter_Should_CreateAuditEntry()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendAndDeadLetterSubscriptionMessageAsync(TopicName, SubscriptionName, "Audit DLQ");
        AuditLogger.Clear();

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        await actor.PeekDeadLetterAsync();

        var entries = AuditLogger.Entries.Where(e => e.Action == "PeekDeadLetterMessagesAsync").ToList();
        Assert.Single(entries);
    }
}
