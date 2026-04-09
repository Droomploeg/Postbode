using Droomploeg.DreamOps.Aspire.Tests.Infrastructure;

namespace Droomploeg.DreamOps.Aspire.Tests.Pages.SubscriptionDetailPageTests.Features;

/// <summary>
/// Tests for resubmit-all operations on the SubscriptionDetailPage.
/// These are bulk operations dispatched to the background worker.
/// </summary>
public class ResubmitAllMessagesTests : DreamOpsTestBase
{
    private const string TopicName = "test-topic-resubmitall";
    private const string SubscriptionName = "sub-resubmitall";

    [Fact]
    public async Task ResubmitAll_Should_OpenAndCloseOverlay()
    {
        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.ResubmitAllAsync();

        Assert.False(actor.HasOpenOverlays);
    }

    // ===== Worker dispatch =====

    [Fact]
    public async Task ResubmitAll_Should_DispatchWorkerItem()
    {
        WorkerDispatcher.Clear();
        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.ResubmitAllAsync();

        Assert.Single(WorkerDispatcher.DispatchedItems);
        Assert.Contains("Resubmit all message", WorkerDispatcher.DispatchedItems[0].Description);
    }

    [Fact]
    public async Task ResubmitAll_WithGenerateIds_Should_DispatchWorkerItem()
    {
        WorkerDispatcher.Clear();
        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.ResubmitAllAsync(generateMessageIds: true);

        Assert.Single(WorkerDispatcher.DispatchedItems);
    }

    [Fact]
    public async Task ResubmitAll_WithDeleteAfterResubmit_Should_DispatchWorkerItem()
    {
        WorkerDispatcher.Clear();
        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.ResubmitAllAsync(deleteMessages: true);

        Assert.Single(WorkerDispatcher.DispatchedItems);
    }

    // ===== Audit =====

    [Fact]
    public async Task ResubmitAll_Should_CreateAuditEntry()
    {
        AuditLogger.Clear();
        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.ResubmitAllAsync();

        var entries = AuditLogger.Entries
            .Where(e => e.Action == "ResubmitAllMessagesAsync")
            .ToList();
        Assert.Single(entries);
        Assert.Contains("Bulk operation", entries[0].Details!);
    }

    [Fact]
    public async Task ResubmitAll_Should_IncludeOptionsInAuditDetails()
    {
        AuditLogger.Clear();
        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.ResubmitAllAsync(generateMessageIds: true, deleteMessages: true);

        var entries = AuditLogger.Entries
            .Where(e => e.Action == "ResubmitAllMessagesAsync")
            .ToList();
        Assert.Single(entries);
        Assert.Contains("Options", entries[0].Details!);
    }
}
