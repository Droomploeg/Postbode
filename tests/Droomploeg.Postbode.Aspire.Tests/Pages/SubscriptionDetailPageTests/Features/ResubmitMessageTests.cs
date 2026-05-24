using Droomploeg.Postbode.Aspire.Tests.Infrastructure;
using Droomploeg.Postbode.WebApp.Components.Controls.AzureServiceBus.Models;

namespace Droomploeg.Postbode.Aspire.Tests.Pages.SubscriptionDetailPageTests.Features;

/// <summary>
/// Tests for resubmitting a single dead-letter message on the SubscriptionDetailPage.
/// </summary>
public class ResubmitMessageTests : PostbodeTestBase
{
    private const string TopicName = "test-topic-resubmit";
    private const string SubscriptionName = "sub-resubmit";

    // ===== Happy scenarios =====

    [Fact]
    public async Task Resubmit_SingleMessage_Should_SendToActiveSubscription()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendAndDeadLetterSubscriptionMessageAsync(TopicName, SubscriptionName, "Resubmit me");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var dlqMessages = await actor.PeekDeadLetterAsync();
        Assert.Single(dlqMessages);

        var model = new SendMessageModel
        {
            Body = "Resubmit me",
            ContentType = "text/plain",
            DeleteMessageAfterResubmit = true
        };
        await actor.ResubmitAsync(dlqMessages.First(), model);

        var activeMessages = await Fixture.PeekSubscriptionMessagesAsync(TopicName, SubscriptionName);
        Assert.Single(activeMessages);
        Assert.Equal("Resubmit me", activeMessages[0].Body.ToString());
    }

    [Fact]
    public async Task Resubmit_WithModifiedBody_Should_SendModifiedMessage()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendAndDeadLetterSubscriptionMessageAsync(TopicName, SubscriptionName, "Original body");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var dlqMessages = await actor.PeekDeadLetterAsync();

        var model = new SendMessageModel
        {
            Body = "Modified body",
            ContentType = "application/json",
            DeleteMessageAfterResubmit = true
        };
        await actor.ResubmitAsync(dlqMessages.First(), model);

        var activeMessages = await Fixture.PeekSubscriptionMessagesAsync(TopicName, SubscriptionName);
        Assert.Single(activeMessages);
        Assert.Equal("Modified body", activeMessages[0].Body.ToString());
    }

    [Fact]
    public async Task Resubmit_WithDeleteAfterResubmit_Should_RemoveFromDeadLetterQueue()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendAndDeadLetterSubscriptionMessageAsync(TopicName, SubscriptionName, "Delete after resubmit");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var dlqMessages = await actor.PeekDeadLetterAsync();

        var model = new SendMessageModel
        {
            Body = "Delete after resubmit",
            ContentType = "text/plain",
            DeleteMessageAfterResubmit = true
        };
        await actor.ResubmitAsync(dlqMessages.First(), model);

        var dlqCount = await Fixture.CountSubscriptionDeadLetterMessagesAsync(TopicName, SubscriptionName);
        Assert.Equal(0, dlqCount);
    }

    [Fact]
    public async Task Resubmit_Should_CloseOverlay()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendAndDeadLetterSubscriptionMessageAsync(TopicName, SubscriptionName, "Overlay close test");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var dlqMessages = await actor.PeekDeadLetterAsync();

        var model = new SendMessageModel
        {
            Body = "Overlay close test",
            ContentType = "text/plain",
            DeleteMessageAfterResubmit = true
        };
        await actor.ResubmitAsync(dlqMessages.First(), model);

        Assert.False(actor.HasOpenOverlays);
    }

    // ===== Unhappy scenarios =====

    [Fact]
    public async Task Resubmit_WithNoSelectedMessage_Should_NotSendAnything()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendAndDeadLetterSubscriptionMessageAsync(TopicName, SubscriptionName, "Should stay in DLQ");

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        Assert.False(actor.HasOpenOverlays);

        var activeCount = await Fixture.CountSubscriptionActiveMessagesAsync(TopicName, SubscriptionName);
        Assert.Equal(0, activeCount);

        var dlqCount = await Fixture.CountSubscriptionDeadLetterMessagesAsync(TopicName, SubscriptionName);
        Assert.Equal(1, dlqCount);
    }

    // ===== Audit scenarios =====

    [Fact]
    public async Task Resubmit_SingleMessage_Should_CreateAuditEntry()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        await Fixture.SendAndDeadLetterSubscriptionMessageAsync(TopicName, SubscriptionName, "Audit resubmit test");
        AuditLogger.Clear();

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var dlqMessages = await actor.PeekDeadLetterAsync();

        var model = new SendMessageModel
        {
            Body = "Audit resubmit test",
            ContentType = "text/plain",
            DeleteMessageAfterResubmit = true
        };
        await actor.ResubmitAsync(dlqMessages.First(), model);

        var resubmitEntries = AuditLogger.Entries
            .Where(e => e.Action == "ResubmitMessageAsync")
            .ToList();
        Assert.Single(resubmitEntries);
        Assert.Contains("Options", resubmitEntries[0].Details!);
    }
}
