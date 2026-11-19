using Droomploeg.Postbode.Aspire.Tests.Infrastructure;
using Droomploeg.Postbode.Domain.Workers.Types;

namespace Droomploeg.Postbode.Aspire.Tests.Pages.QueueDetailPageTests.Features;

/// <summary>
/// Runs the background job of a delete-all action against the emulator. The job gets its own
/// scope and Service Bus client when it starts, so it must still work after the page is gone.
/// </summary>
public class DeleteAllMessagesJobTests : PostbodeTestBase
{
    private const string QueueName = "test-delete-all-job";

    [Fact]
    public async Task DeleteAllActiveJob_Should_RemoveMessages_AfterPageIsDisposed()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendMessageAsync(QueueName, "first");
        await Fixture.SendMessageAsync(QueueName, "second");
        await Fixture.SendMessageAsync(QueueName, "third");
        WorkerDispatcher.Clear();

        var actor = DetailPageActor.Create(this, QueueName);
        await actor.DeleteAllActiveAsync();
        var job = Assert.Single(WorkerDispatcher.DispatchedItems);

        // The user leaves the page before the job starts.
        await DisposeComponentsAsync();

        // Act
        await job.ExecuteAsync();

        // Assert
        Assert.Equal(WorkItemState.Completed, job.State);
        Assert.Equal(0, await Fixture.CountActiveMessagesAsync(QueueName));
    }
}
