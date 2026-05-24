using Droomploeg.Postbode.Aspire.Tests.Infrastructure;

namespace Droomploeg.Postbode.Aspire.Tests.Pages.QueueDetailPageTests.Features;

/// <summary>
/// Tests for the delete active message functionality on the QueueDetailPage.
/// Flow: user peeks messages, selects one, clicks delete, confirms in dialog.
/// </summary>
public class DeleteActiveMessageTests : PostbodeTestBase
{
    private const string QueueName = "test-delete-message";

    // ===== Happy scenarios =====

    [Fact]
    public async Task Delete_SingleMessage_Should_RemoveFromQueue()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendMessageAsync(QueueName, "Message to delete");

        var actor = DetailPageActor.Create(this, QueueName);
        var messages = await actor.PeekActiveAsync();
        Assert.Single(messages);

        // Act
        await actor.DeleteAsync(messages.First());

        // Assert
        var remaining = await Fixture.CountActiveMessagesAsync(QueueName);
        Assert.Equal(0, remaining);
    }

    [Fact]
    public async Task Delete_SingleMessage_Should_CloseDialog()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendMessageAsync(QueueName, "Message to delete");

        var actor = DetailPageActor.Create(this, QueueName);
        var messages = await actor.PeekActiveAsync();

        // Act
        await actor.DeleteAsync(messages.First());

        // Assert — dialog should be closed (no dialog-container in DOM)
        Assert.False(actor.HasVisibleDialogs);
    }

    [Fact]
    public async Task Delete_FirstOfMultipleMessages_Should_OnlyRemoveOne()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendMessageAsync(QueueName, "Message 1");
        await Fixture.SendMessageAsync(QueueName, "Message 2");
        await Fixture.SendMessageAsync(QueueName, "Message 3");

        var actor = DetailPageActor.Create(this, QueueName);
        var messages = await actor.PeekActiveAsync();
        Assert.Equal(3, messages.Count);

        // Act — delete first message
        await actor.DeleteAsync(messages.First());

        // Assert
        var remaining = await Fixture.CountActiveMessagesAsync(QueueName);
        Assert.Equal(2, remaining);
    }

    // ===== Unhappy scenarios =====

    [Fact]
    public async Task Delete_WithNoSelectedMessage_Should_NotRemoveAnything()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendMessageAsync(QueueName, "Should stay");

        var actor = DetailPageActor.Create(this, QueueName);

        // Act — try to click Confirm without opening a delete dialog first
        // The page's DeleteSelectedMessageAsync returns early if _selectedMessage is null
        // No dialog visible, so no confirm buttons should exist
        Assert.False(actor.HasConfirmButton);

        // Assert — message still on queue
        var count = await Fixture.CountActiveMessagesAsync(QueueName);
        Assert.Equal(1, count);
    }

    // ===== Audit scenarios =====

    [Fact]
    public async Task Delete_SingleMessage_Should_CreateAuditEntry()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendMessageAsync(QueueName, "Audit delete test");
        AuditLogger.Clear();

        var actor = DetailPageActor.Create(this, QueueName);
        var messages = await actor.PeekActiveAsync();

        // Act
        await actor.DeleteAsync(messages.First());

        // Assert
        var deleteEntries = AuditLogger.Entries
            .Where(e => e.Action == "DeleteActiveMessageAsync")
            .ToList();
        Assert.Single(deleteEntries);
        Assert.Equal(QueueName, deleteEntries[0].Resource);
    }
}
