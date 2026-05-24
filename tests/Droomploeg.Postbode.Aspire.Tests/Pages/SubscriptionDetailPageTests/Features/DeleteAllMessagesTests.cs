using Droomploeg.Postbode.Aspire.Tests.Infrastructure;

namespace Droomploeg.Postbode.Aspire.Tests.Pages.SubscriptionDetailPageTests.Features;

/// <summary>
/// Tests for delete-all operations on the SubscriptionDetailPage.
/// These are bulk operations dispatched to the background worker.
/// </summary>
public class DeleteAllMessagesTests : PostbodeTestBase
{
    private const string TopicName = "test-topic-deleteall";
    private const string SubscriptionName = "sub-deleteall";

    [Fact]
    public async Task DeleteAllActive_Should_OpenAndCloseDialog()
    {
        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.DeleteAllActiveAsync();

        Assert.False(actor.HasVisibleDialogs);
    }

    [Fact]
    public async Task DeleteAllDeadLetter_Should_OpenAndCloseDialog()
    {
        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.DeleteAllDeadLetterAsync();

        Assert.False(actor.HasVisibleDialogs);
    }

    // ===== Worker dispatch =====

    [Fact]
    public async Task DeleteAllActive_Should_DispatchWorkerItem()
    {
        WorkerDispatcher.Clear();
        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.DeleteAllActiveAsync();

        Assert.Single(WorkerDispatcher.DispatchedItems);
        Assert.Contains("Delete all message", WorkerDispatcher.DispatchedItems[0].Description);
    }

    [Fact]
    public async Task DeleteAllDeadLetter_Should_DispatchWorkerItem()
    {
        WorkerDispatcher.Clear();
        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.DeleteAllDeadLetterAsync();

        Assert.Single(WorkerDispatcher.DispatchedItems);
        Assert.Contains("Delete all message", WorkerDispatcher.DispatchedItems[0].Description);
    }

    // ===== Audit =====

    [Fact]
    public async Task DeleteAllActive_Should_CreateAuditEntry()
    {
        AuditLogger.Clear();
        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.DeleteAllActiveAsync();

        var entries = AuditLogger.Entries
            .Where(e => e.Details == "Bulk operation")
            .ToList();
        Assert.Single(entries);
    }

    [Fact]
    public async Task DeleteAllDeadLetter_Should_CreateAuditEntry()
    {
        AuditLogger.Clear();
        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.DeleteAllDeadLetterAsync();

        var entries = AuditLogger.Entries
            .Where(e => e.Action == "DeleteAllDeadLetterMessagesAsync")
            .ToList();
        Assert.Single(entries);
        Assert.Equal("Bulk operation", entries[0].Details);
    }
}
