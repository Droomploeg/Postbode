using Droomploeg.Postbode.Aspire.Tests.Infrastructure;

namespace Droomploeg.Postbode.Aspire.Tests.Pages.QueueDetailPageTests.Features;

/// <summary>
/// Tests for delete-all operations on the QueueDetailPage.
/// These are bulk operations dispatched to the background worker.
/// Verification: a WorkerItem is dispatched + audit entry is created.
/// </summary>
public class DeleteAllMessagesTests : PostbodeTestBase
{
    private const string QueueName = "test-delete-all";

    // ===== Happy scenarios - Active =====

    [Fact]
    public async Task DeleteAllActive_Should_DispatchWorkerItem()
    {
        // Arrange
        WorkerDispatcher.Clear();
        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.DeleteAllActiveAsync();

        // Assert
        Assert.Single(WorkerDispatcher.DispatchedItems);
        Assert.Equal(QueueName, WorkerDispatcher.DispatchedItems[0].Entity);
        Assert.Contains("Delete all message", WorkerDispatcher.DispatchedItems[0].Description);
    }

    [Fact]
    public async Task DeleteAllActive_Should_CloseDialog()
    {
        // Arrange
        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.DeleteAllActiveAsync();

        // Assert
        Assert.False(actor.HasVisibleDialogs);
    }

    // ===== Happy scenarios - Dead-letter =====

    [Fact]
    public async Task DeleteAllDeadLetter_Should_DispatchWorkerItem()
    {
        // Arrange
        WorkerDispatcher.Clear();
        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.DeleteAllDeadLetterAsync();

        // Assert
        Assert.Single(WorkerDispatcher.DispatchedItems);
        Assert.Equal(QueueName, WorkerDispatcher.DispatchedItems[0].Entity);
        Assert.Contains("Delete all dead letter", WorkerDispatcher.DispatchedItems[0].Description);
    }

    // ===== Audit scenarios =====

    [Fact]
    public async Task DeleteAllActive_Should_CreateAuditEntry()
    {
        // Arrange
        AuditLogger.Clear();
        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.DeleteAllActiveAsync();

        // Assert
        var entries = AuditLogger.Entries
            .Where(e => e.Details == "Bulk operation")
            .ToList();
        Assert.Single(entries);
        Assert.Equal(QueueName, entries[0].Resource);
    }

    [Fact]
    public async Task DeleteAllDeadLetter_Should_CreateAuditEntry()
    {
        // Arrange
        AuditLogger.Clear();
        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.DeleteAllDeadLetterAsync();

        // Assert
        var entries = AuditLogger.Entries
            .Where(e => e.Action == "DeleteAllDeadLetterMessagesAsync")
            .ToList();
        Assert.Single(entries);
        Assert.Equal(QueueName, entries[0].Resource);
        Assert.Equal("Bulk operation", entries[0].Details);
    }
}
