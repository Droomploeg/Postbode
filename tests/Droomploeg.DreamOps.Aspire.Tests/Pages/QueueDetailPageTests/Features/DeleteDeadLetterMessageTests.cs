using Droomploeg.DreamOps.Aspire.Tests.Infrastructure;

namespace Droomploeg.DreamOps.Aspire.Tests.Pages.QueueDetailPageTests.Features;

/// <summary>
/// Tests for deleting a single dead-letter message on the QueueDetailPage.
/// Flow: user switches to dead-letter tab, peeks messages, selects one, clicks delete, confirms in dialog.
/// </summary>
public class DeleteDeadLetterMessageTests : DreamOpsTestBase
{
    private const string QueueName = "test-delete-deadletter";

    // ===== Happy scenarios =====

    [Fact]
    public async Task Delete_SingleDeadLetterMessage_Should_RemoveFromQueue()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendAndDeadLetterMessageAsync(QueueName, "DLQ message to delete");

        var actor = DetailPageActor.Create(this, QueueName);
        var messages = await actor.PeekDeadLetterAsync();
        Assert.Single(messages);

        // Act
        await actor.DeleteAsync(messages.First());

        // Assert
        var remaining = await Fixture.CountDeadLetterMessagesAsync(QueueName);
        Assert.Equal(0, remaining);
    }

    [Fact]
    public async Task Delete_SingleDeadLetterMessage_Should_CloseDialog()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendAndDeadLetterMessageAsync(QueueName, "DLQ message to delete");

        var actor = DetailPageActor.Create(this, QueueName);
        var messages = await actor.PeekDeadLetterAsync();

        // Act
        await actor.DeleteAsync(messages.First());

        // Assert
        Assert.False(actor.HasVisibleDialogs);
    }

    [Fact]
    public async Task Delete_FirstOfMultipleDeadLetterMessages_Should_OnlyRemoveOne()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendAndDeadLetterMessageAsync(QueueName, "DLQ message 1");
        await Fixture.SendAndDeadLetterMessageAsync(QueueName, "DLQ message 2");
        await Fixture.SendAndDeadLetterMessageAsync(QueueName, "DLQ message 3");

        var actor = DetailPageActor.Create(this, QueueName);
        var messages = await actor.PeekDeadLetterAsync();
        Assert.Equal(3, messages.Count);

        // Act
        await actor.DeleteAsync(messages.First());

        // Assert
        var remaining = await Fixture.CountDeadLetterMessagesAsync(QueueName);
        Assert.Equal(2, remaining);
    }

    [Fact]
    public async Task Delete_DeadLetterMessage_Should_NotAffectActiveMessages()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendMessageAsync(QueueName, "Active message");
        await Fixture.SendAndDeadLetterMessageAsync(QueueName, "DLQ message");

        var actor = DetailPageActor.Create(this, QueueName);
        var dlqMessages = await actor.PeekDeadLetterAsync();

        // Act
        await actor.DeleteAsync(dlqMessages.First());

        // Assert — active message should still be there
        var activeCount = await Fixture.CountActiveMessagesAsync(QueueName);
        Assert.Equal(1, activeCount);

        var dlqCount = await Fixture.CountDeadLetterMessagesAsync(QueueName);
        Assert.Equal(0, dlqCount);
    }

    // ===== Audit scenarios =====

    [Fact]
    public async Task Delete_SingleDeadLetterMessage_Should_CreateAuditEntry()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendAndDeadLetterMessageAsync(QueueName, "Audit DLQ delete test");
        AuditLogger.Clear();

        var actor = DetailPageActor.Create(this, QueueName);
        var messages = await actor.PeekDeadLetterAsync();

        // Act
        await actor.DeleteAsync(messages.First());

        // Assert
        var deleteEntries = AuditLogger.Entries
            .Where(e => e.Action == "DeleteDeadLetterMessageAsync")
            .ToList();
        Assert.Single(deleteEntries);
        Assert.Equal(QueueName, deleteEntries[0].Resource);
    }
}
