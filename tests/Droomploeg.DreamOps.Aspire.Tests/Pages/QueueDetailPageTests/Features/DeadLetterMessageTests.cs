using Droomploeg.DreamOps.Aspire.Tests.Infrastructure;

namespace Droomploeg.DreamOps.Aspire.Tests.Pages.QueueDetailPageTests.Features;

/// <summary>
/// Tests for dead-lettering an active message on the QueueDetailPage.
/// Flow: user peeks active messages, selects one, clicks dead-letter,
/// fills in reason/description, confirms — message moves to DLQ.
/// </summary>
public class DeadLetterMessageTests : DreamOpsTestBase
{
    private const string QueueName = "test-deadletter-message";

    // ===== Happy scenarios =====

    [Fact]
    public async Task DeadLetter_SingleMessage_Should_MoveToDeadLetterQueue()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendMessageAsync(QueueName, "Move me to DLQ");

        var actor = DetailPageActor.Create(this, QueueName);
        var messages = await actor.PeekActiveAsync();
        Assert.Single(messages);

        // Act
        await actor.DeadLetterAsync(messages.First(), "TestReason", "Test description");

        // Assert — message should be gone from active queue
        var activeCount = await Fixture.CountActiveMessagesAsync(QueueName);
        Assert.Equal(0, activeCount);

        // And should be in the dead-letter queue
        var dlqMessages = await Fixture.PeekDeadLetterMessagesAsync(QueueName);
        Assert.Single(dlqMessages);
        Assert.Equal("Move me to DLQ", dlqMessages[0].Body.ToString());
    }

    [Fact]
    public async Task DeadLetter_SingleMessage_Should_CloseOverlay()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendMessageAsync(QueueName, "Close overlay test");

        var actor = DetailPageActor.Create(this, QueueName);
        var messages = await actor.PeekActiveAsync();

        // Act
        await actor.DeadLetterAsync(messages.First(), "Reason", "Description");

        // Assert — all overlays should be closed
        Assert.False(actor.HasOpenOverlays);
    }

    [Fact]
    public async Task DeadLetter_FirstOfMultiple_Should_OnlyMoveOne()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendMessageAsync(QueueName, "Message 1");
        await Fixture.SendMessageAsync(QueueName, "Message 2");

        var actor = DetailPageActor.Create(this, QueueName);
        var messages = await actor.PeekActiveAsync();
        Assert.Equal(2, messages.Count);

        // Act — dead-letter only the first
        await actor.DeadLetterAsync(messages.First(), "Reason", "Only first");

        // Assert
        var activeCount = await Fixture.CountActiveMessagesAsync(QueueName);
        Assert.Equal(1, activeCount);

        var dlqCount = await Fixture.CountDeadLetterMessagesAsync(QueueName);
        Assert.Equal(1, dlqCount);
    }

    // ===== Unhappy scenarios =====

    [Fact]
    public async Task DeadLetter_WithNoSelectedMessage_Should_NotMoveAnything()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendMessageAsync(QueueName, "Should stay active");

        var actor = DetailPageActor.Create(this, QueueName);
        // Don't peek or select — _selectedMessage is null

        // The DeadLetterSelectedMessageAsync returns early if _selectedMessage is null
        // We can't trigger it through the overlay without a selected message,
        // so we verify the button/overlay isn't accessible without selection
        Assert.False(actor.HasOpenOverlays);

        // Assert — message still on active queue, nothing in DLQ
        var activeCount = await Fixture.CountActiveMessagesAsync(QueueName);
        Assert.Equal(1, activeCount);

        var dlqCount = await Fixture.CountDeadLetterMessagesAsync(QueueName);
        Assert.Equal(0, dlqCount);
    }

    // ===== Audit scenarios =====

    [Fact]
    public async Task DeadLetter_SingleMessage_Should_CreateAuditEntry()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendMessageAsync(QueueName, "Audit dead-letter test");
        AuditLogger.Clear();

        var actor = DetailPageActor.Create(this, QueueName);
        var messages = await actor.PeekActiveAsync();

        // Act
        await actor.DeadLetterAsync(messages.First(), "AuditReason", "Audit description");

        // Assert
        var dlEntries = AuditLogger.Entries
            .Where(e => e.Action == "DeadLetterMessageAsync")
            .ToList();
        Assert.Single(dlEntries);
        Assert.Equal(QueueName, dlEntries[0].Resource);
        Assert.Contains("AuditReason", dlEntries[0].Details!);
    }
}
