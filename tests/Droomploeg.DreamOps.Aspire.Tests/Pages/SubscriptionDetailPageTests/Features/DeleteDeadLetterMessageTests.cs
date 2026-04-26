using Droomploeg.DreamOps.Aspire.Tests.Infrastructure;

namespace Droomploeg.DreamOps.Aspire.Tests.Pages.SubscriptionDetailPageTests.Features;

/// <summary>
/// Tests for deleting a single dead-letter message on the SubscriptionDetailPage.
/// Flow: user switches to dead-letter tab, peeks messages, selects one, clicks delete, confirms in dialog.
/// </summary>
public class DeleteDeadLetterMessageTests : DreamOpsTestBase
{
    private const string TopicName = "test-topic-delete-dl";
    private const string SubscriptionName = "sub-delete-dl";

    // ===== Happy scenarios =====

    [Fact]
    public async Task Delete_SingleDeadLetterMessage_Should_RemoveFromSubscription()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendAndDeadLetterSubscriptionMessageAsync(TopicName, SubscriptionName, "DLQ message to delete");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekDeadLetterAsync();
        Assert.Single(messages);

        await actor.DeleteAsync(messages.First());

        var remaining = await Fixture.CountSubscriptionDeadLetterMessagesAsync(TopicName, SubscriptionName);
        Assert.Equal(0, remaining);
    }

    [Fact]
    public async Task Delete_SingleDeadLetterMessage_Should_CloseDialog()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendAndDeadLetterSubscriptionMessageAsync(TopicName, SubscriptionName, "DLQ message to delete");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekDeadLetterAsync();

        await actor.DeleteAsync(messages.First());

        Assert.False(actor.HasVisibleDialogs);
    }

    [Fact]
    public async Task Delete_FirstOfMultipleDeadLetterMessages_Should_OnlyRemoveOne()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendAndDeadLetterSubscriptionMessageAsync(TopicName, SubscriptionName, "DLQ 1");
        await Fixture.SendAndDeadLetterSubscriptionMessageAsync(TopicName, SubscriptionName, "DLQ 2");
        await Fixture.SendAndDeadLetterSubscriptionMessageAsync(TopicName, SubscriptionName, "DLQ 3");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekDeadLetterAsync();
        Assert.Equal(3, messages.Count);

        await actor.DeleteAsync(messages.First());

        var remaining = await Fixture.CountSubscriptionDeadLetterMessagesAsync(TopicName, SubscriptionName);
        Assert.Equal(2, remaining);
    }

    [Fact]
    public async Task Delete_DeadLetterMessage_Should_NotAffectActiveMessages()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendTopicMessageAsync(TopicName, "Active message");
        await Fixture.SendAndDeadLetterSubscriptionMessageAsync(TopicName, SubscriptionName, "DLQ message");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var dlqMessages = await actor.PeekDeadLetterAsync();

        await actor.DeleteAsync(dlqMessages.First());

        var activeCount = await Fixture.CountSubscriptionActiveMessagesAsync(TopicName, SubscriptionName);
        Assert.Equal(1, activeCount);

        var dlqCount = await Fixture.CountSubscriptionDeadLetterMessagesAsync(TopicName, SubscriptionName);
        Assert.Equal(0, dlqCount);
    }

    // ===== Audit scenarios =====

    [Fact]
    public async Task Delete_SingleDeadLetterMessage_Should_CreateAuditEntry()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendAndDeadLetterSubscriptionMessageAsync(TopicName, SubscriptionName, "Audit DLQ delete");
        AuditLogger.Clear();

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekDeadLetterAsync();

        await actor.DeleteAsync(messages.First());

        var entries = AuditLogger.Entries
            .Where(e => e.Action == "DeleteDeadLetterMessageAsync")
            .ToList();
        Assert.Single(entries);
    }
}
