using Droomploeg.DreamOps.Aspire.Tests.Infrastructure;
using Droomploeg.DreamOps.WebApp.Components.Controls.AzureServiceBus.Models;

namespace Droomploeg.DreamOps.Aspire.Tests.Pages.QueueDetailPageTests.Features;

/// <summary>
/// Tests for resubmitting a single dead-letter message on the QueueDetailPage.
/// Flow: user switches to dead-letter tab, peeks messages, selects one, clicks resubmit,
/// optionally modifies the message body/properties, confirms — message is sent back to active queue.
/// </summary>
public class ResubmitMessageTests : DreamOpsTestBase
{
    private const string QueueName = "test-resubmit-deadletter";

    // ===== Happy scenarios =====

    [Fact]
    public async Task Resubmit_SingleMessage_Should_SendToActiveQueue()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendAndDeadLetterMessageAsync(QueueName, "Resubmit me");

        var actor = DetailPageActor.Create(this, QueueName);
        var dlqMessages = await actor.PeekDeadLetterAsync();
        Assert.Single(dlqMessages);

        // Act — resubmit with the same body
        var model = new SendMessageModel
        {
            Body = "Resubmit me",
            ContentType = "text/plain",
            DeleteMessageAfterResubmit = true
        };
        await actor.ResubmitAsync(dlqMessages.First(), model);

        // Assert — message should appear on active queue
        var activeMessages = await Fixture.PeekActiveMessagesAsync(QueueName);
        Assert.Single(activeMessages);
        Assert.Equal("Resubmit me", activeMessages[0].Body.ToString());
    }

    [Fact]
    public async Task Resubmit_WithModifiedBody_Should_SendModifiedMessage()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendAndDeadLetterMessageAsync(QueueName, "Original body");

        var actor = DetailPageActor.Create(this, QueueName);
        var dlqMessages = await actor.PeekDeadLetterAsync();

        // Act — resubmit with modified body
        var model = new SendMessageModel
        {
            Body = "Modified body",
            ContentType = "application/json",
            DeleteMessageAfterResubmit = true
        };
        await actor.ResubmitAsync(dlqMessages.First(), model);

        // Assert
        var activeMessages = await Fixture.PeekActiveMessagesAsync(QueueName);
        Assert.Single(activeMessages);
        Assert.Equal("Modified body", activeMessages[0].Body.ToString());
    }

    [Fact]
    public async Task Resubmit_WithDeleteAfterResubmit_Should_RemoveFromDeadLetterQueue()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendAndDeadLetterMessageAsync(QueueName, "Delete after resubmit");

        var actor = DetailPageActor.Create(this, QueueName);
        var dlqMessages = await actor.PeekDeadLetterAsync();

        // Act
        var model = new SendMessageModel
        {
            Body = "Delete after resubmit",
            ContentType = "text/plain",
            DeleteMessageAfterResubmit = true
        };
        await actor.ResubmitAsync(dlqMessages.First(), model);

        // Assert — DLQ should be empty
        var dlqCount = await Fixture.CountDeadLetterMessagesAsync(QueueName);
        Assert.Equal(0, dlqCount);
    }

    [Fact]
    public async Task Resubmit_Should_CloseOverlay()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendAndDeadLetterMessageAsync(QueueName, "Overlay close test");

        var actor = DetailPageActor.Create(this, QueueName);
        var dlqMessages = await actor.PeekDeadLetterAsync();

        // Act
        var model = new SendMessageModel
        {
            Body = "Overlay close test",
            ContentType = "text/plain",
            DeleteMessageAfterResubmit = true
        };
        await actor.ResubmitAsync(dlqMessages.First(), model);

        // Assert
        Assert.False(actor.HasOpenOverlays);
    }

    // ===== Unhappy scenarios =====

    [Fact]
    public async Task Resubmit_WithNoSelectedMessage_Should_NotSendAnything()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendAndDeadLetterMessageAsync(QueueName, "Should stay in DLQ");

        var actor = DetailPageActor.Create(this, QueueName);
        // Don't peek or select — _selectedMessage is null

        // No resubmit overlay should be open
        Assert.False(actor.HasOpenOverlays);

        // Assert — nothing on active queue
        var activeCount = await Fixture.CountActiveMessagesAsync(QueueName);
        Assert.Equal(0, activeCount);

        // DLQ message still there
        var dlqCount = await Fixture.CountDeadLetterMessagesAsync(QueueName);
        Assert.Equal(1, dlqCount);
    }

    // ===== Audit scenarios =====

    [Fact]
    public async Task Resubmit_SingleMessage_Should_CreateAuditEntry()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendAndDeadLetterMessageAsync(QueueName, "Audit resubmit test");
        AuditLogger.Clear();

        var actor = DetailPageActor.Create(this, QueueName);
        var dlqMessages = await actor.PeekDeadLetterAsync();

        // Act
        var model = new SendMessageModel
        {
            Body = "Audit resubmit test",
            ContentType = "text/plain",
            DeleteMessageAfterResubmit = true
        };
        await actor.ResubmitAsync(dlqMessages.First(), model);

        // Assert
        var resubmitEntries = AuditLogger.Entries
            .Where(e => e.Action == "ResubmitMessageAsync")
            .ToList();
        Assert.Single(resubmitEntries);
        Assert.Equal(QueueName, resubmitEntries[0].Resource);
        Assert.Contains("Options", resubmitEntries[0].Details!);
    }
}
