using Droomploeg.DreamOps.Aspire.Tests.Infrastructure;

namespace Droomploeg.DreamOps.Aspire.Tests.Pages.QueueDetailPageTests.Features;

/// <summary>
/// Tests for resubmit-all operations on the QueueDetailPage.
/// Flow: user switches to dead-letter tab, clicks "Re-Submit All",
/// fills in options (generate IDs, delete after resubmit), confirms.
/// This is a bulk operation dispatched to the background worker.
/// </summary>
public class ResubmitAllMessagesTests : DreamOpsTestBase
{
    private const string QueueName = "test-resubmit-deadletter";

    // ===== Happy scenarios =====

    [Fact]
    public async Task ResubmitAll_Should_DispatchWorkerItem()
    {
        // Arrange
        WorkerDispatcher.Clear();
        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.ResubmitAllAsync();

        // Assert
        Assert.Single(WorkerDispatcher.DispatchedItems);
        Assert.Equal(QueueName, WorkerDispatcher.DispatchedItems[0].Entity);
        Assert.Contains("Resubmit all dead letter", WorkerDispatcher.DispatchedItems[0].Description);
    }

    [Fact]
    public async Task ResubmitAll_WithGenerateIds_Should_DispatchWorkerItem()
    {
        // Arrange
        WorkerDispatcher.Clear();
        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.ResubmitAllAsync(generateMessageIds: true);

        // Assert
        Assert.Single(WorkerDispatcher.DispatchedItems);
    }

    [Fact]
    public async Task ResubmitAll_WithDeleteAfterResubmit_Should_DispatchWorkerItem()
    {
        // Arrange
        WorkerDispatcher.Clear();
        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.ResubmitAllAsync(deleteMessages: true);

        // Assert
        Assert.Single(WorkerDispatcher.DispatchedItems);
    }

    [Fact]
    public async Task ResubmitAll_Should_CloseOverlay()
    {
        // Arrange
        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.ResubmitAllAsync();

        // Assert
        Assert.False(actor.HasOpenOverlays);
    }

    // ===== Audit scenarios =====

    [Fact]
    public async Task ResubmitAll_Should_CreateAuditEntry()
    {
        // Arrange
        AuditLogger.Clear();
        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.ResubmitAllAsync();

        // Assert
        var entries = AuditLogger.Entries
            .Where(e => e.Action == "ResubmitAllMessagesAsync")
            .ToList();
        Assert.Single(entries);
        Assert.Equal(QueueName, entries[0].Resource);
        Assert.Contains("Bulk operation", entries[0].Details!);
    }

    [Fact]
    public async Task ResubmitAll_Should_IncludeOptionsInAuditDetails()
    {
        // Arrange
        AuditLogger.Clear();
        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.ResubmitAllAsync(generateMessageIds: true, deleteMessages: true);

        // Assert
        var entries = AuditLogger.Entries
            .Where(e => e.Action == "ResubmitAllMessagesAsync")
            .ToList();
        Assert.Single(entries);
        Assert.Contains("Options", entries[0].Details!);
    }
}
