using Droomploeg.Postbode.Aspire.Tests.Infrastructure;

namespace Droomploeg.Postbode.Aspire.Tests.Pages.SubscriptionDetailPageTests.Features;

public class DeleteActiveMessageTests : PostbodeTestBase
{
    private const string TopicName = "test-topic-delete";
    private const string SubscriptionName = "sub-delete";

    // ===== Happy scenarios =====

    [Fact]
    public async Task Delete_SingleMessage_Should_RemoveFromSubscription()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendTopicMessageAsync(TopicName, "Delete me");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekActiveAsync();
        Assert.Single(messages);

        await actor.DeleteAsync(messages.First());

        var remaining = await Fixture.CountSubscriptionActiveMessagesAsync(TopicName, SubscriptionName);
        Assert.Equal(0, remaining);
    }

    [Fact]
    public async Task Delete_SingleMessage_Should_CloseDialog()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendTopicMessageAsync(TopicName, "Delete me");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekActiveAsync();

        await actor.DeleteAsync(messages.First());

        Assert.False(actor.HasVisibleDialogs);
    }

    [Fact]
    public async Task Delete_FirstOfMultiple_Should_OnlyRemoveOne()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendTopicMessageAsync(TopicName, "Msg 1");
        await Fixture.SendTopicMessageAsync(TopicName, "Msg 2");
        await Fixture.SendTopicMessageAsync(TopicName, "Msg 3");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekActiveAsync();
        Assert.Equal(3, messages.Count);

        await actor.DeleteAsync(messages.First());

        var remaining = await Fixture.CountSubscriptionActiveMessagesAsync(TopicName, SubscriptionName);
        Assert.Equal(2, remaining);
    }

    // ===== Unhappy scenarios =====

    [Fact]
    public async Task Delete_WithNoSelectedMessage_Should_NotRemoveAnything()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendTopicMessageAsync(TopicName, "Should stay");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        Assert.False(actor.HasConfirmButton);

        var count = await Fixture.CountSubscriptionActiveMessagesAsync(TopicName, SubscriptionName);
        Assert.Equal(1, count);
    }

    // ===== Audit =====

    [Fact]
    public async Task Delete_SingleMessage_Should_CreateAuditEntry()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendTopicMessageAsync(TopicName, "Audit delete");
        AuditLogger.Clear();

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekActiveAsync();

        await actor.DeleteAsync(messages.First());

        var entries = AuditLogger.Entries.Where(e => e.Action == "DeleteActiveMessageAsync").ToList();
        Assert.Single(entries);
    }
}
