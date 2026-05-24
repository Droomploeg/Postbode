using Droomploeg.Postbode.Aspire.Tests.Infrastructure;
using Droomploeg.Postbode.Domain.Workers.Types;

namespace Droomploeg.Postbode.Aspire.Tests.Pages.BackgroundJobsPageTests;

/// <summary>
/// Tests for the BackgroundJobsPage.
/// Verifies that bulk operations (delete-all) dispatched from detail pages
/// appear as background jobs with correct state transitions.
/// </summary>
public class BackgroundJobsPageTests : PostbodeTestBase
{
    private const string QueueName = "test-delete-all";
    private const string TopicName = "test-topic-deleteall";
    private const string SubscriptionName = "sub-deleteall";

    // ===== Queue bulk delete =====

    [Fact]
    public async Task DeleteAllActiveQueue_Should_CreateBackgroundJob()
    {
        var queueActor = DetailPageActor.Create(this, QueueName);
        await queueActor.DeleteAllActiveAsync();

        var jobsActor = BackgroundJobsPageActor.Create(this);

        Assert.True(jobsActor.HasWorkerItem("Delete all message"));
    }

    [Fact]
    public async Task DeleteAllDeadLetterQueue_Should_CreateBackgroundJob()
    {
        WorkerDispatcher.Clear();
        var queueActor = DetailPageActor.Create(this, QueueName);
        await queueActor.DeleteAllDeadLetterAsync();

        // Verify dispatcher captured the item
        Assert.Single(WorkerDispatcher.DispatchedItems);

        var jobsActor = BackgroundJobsPageActor.Create(this);

        Assert.True(jobsActor.HasWorkerItem("Delete all"));
    }

    [Fact]
    public async Task DeleteAllActiveQueue_Should_ShowScheduledState()
    {
        var queueActor = DetailPageActor.Create(this, QueueName);
        await queueActor.DeleteAllActiveAsync();

        var jobsActor = BackgroundJobsPageActor.Create(this);

        Assert.True(jobsActor.HasState(WorkItemState.Scheduled.ToString()));
    }

    // ===== Subscription bulk delete =====

    [Fact]
    public async Task DeleteAllActiveSubscription_Should_CreateBackgroundJob()
    {
        var subActor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        await subActor.DeleteAllActiveAsync();

        var jobsActor = BackgroundJobsPageActor.Create(this);

        Assert.True(jobsActor.HasWorkerItem("Delete all message"));
    }

    [Fact]
    public async Task DeleteAllDeadLetterSubscription_Should_CreateBackgroundJob()
    {
        var subActor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        await subActor.DeleteAllDeadLetterAsync();

        var jobsActor = BackgroundJobsPageActor.Create(this);

        Assert.True(jobsActor.HasWorkerItem("Delete all message"));
    }

    [Fact]
    public async Task DeleteAllActiveSubscription_Should_ShowScheduledState()
    {
        var subActor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        await subActor.DeleteAllActiveAsync();

        var jobsActor = BackgroundJobsPageActor.Create(this);

        Assert.True(jobsActor.HasState(WorkItemState.Scheduled.ToString()));
    }

    // ===== Page functionality =====

    [Fact]
    public void Should_RenderRefreshButton()
    {
        var actor = BackgroundJobsPageActor.Create(this);

        Assert.True(actor.HasRefreshButton);
    }

    [Fact]
    public void NoJobs_Should_RenderEmptyGrid()
    {
        var actor = BackgroundJobsPageActor.Create(this);

        Assert.False(actor.HasWorkerItem("Delete"));
    }

    [Fact]
    public async Task MultipleJobs_Should_ShowAll()
    {
        WorkerDispatcher.Clear();
        var queueActor = DetailPageActor.Create(this, QueueName);
        await queueActor.DeleteAllActiveAsync();
        await queueActor.DeleteAllDeadLetterAsync();

        Assert.Equal(2, WorkerDispatcher.DispatchedItems.Count);

        var jobsActor = BackgroundJobsPageActor.Create(this);

        Assert.True(jobsActor.HasWorkerItem("Delete all"));
    }

    // ===== Cancel =====

    [Fact]
    public async Task ScheduledJob_Should_ShowStopButton()
    {
        var queueActor = DetailPageActor.Create(this, QueueName);
        await queueActor.DeleteAllActiveAsync();

        var jobsActor = BackgroundJobsPageActor.Create(this);

        Assert.True(jobsActor.HasStopButton);
    }

    [Fact]
    public async Task CancelScheduledJob_Should_ChangeToCancelledState()
    {
        var queueActor = DetailPageActor.Create(this, QueueName);
        await queueActor.DeleteAllActiveAsync();

        var jobsActor = BackgroundJobsPageActor.Create(this);
        await jobsActor.ClickStopAsync();

        Assert.True(jobsActor.HasState(WorkItemState.Cancelled.ToString()));
    }

    [Fact]
    public async Task CancelScheduledJob_Should_HideStopButton()
    {
        var queueActor = DetailPageActor.Create(this, QueueName);
        await queueActor.DeleteAllActiveAsync();

        var jobsActor = BackgroundJobsPageActor.Create(this);
        await jobsActor.ClickStopAsync();

        Assert.False(jobsActor.HasStopButton);
    }

    [Fact]
    public async Task CancelScheduledJob_Should_ShowDeleteButton()
    {
        var queueActor = DetailPageActor.Create(this, QueueName);
        await queueActor.DeleteAllActiveAsync();

        var jobsActor = BackgroundJobsPageActor.Create(this);
        await jobsActor.ClickStopAsync();

        Assert.True(jobsActor.HasDeleteButton);
    }

    [Fact]
    public async Task DeleteCancelledJob_Should_RemoveFromGrid()
    {
        var queueActor = DetailPageActor.Create(this, QueueName);
        await queueActor.DeleteAllActiveAsync();

        var jobsActor = BackgroundJobsPageActor.Create(this);
        await jobsActor.ClickStopAsync();
        await jobsActor.ClickDeleteJobAsync();

        Assert.False(jobsActor.HasWorkerItem("Delete all"));
    }

    // ===== Queue ResubmitAll =====

    [Fact]
    public async Task ResubmitAllQueue_Should_CreateBackgroundJob()
    {
        var queueActor = DetailPageActor.Create(this, QueueName);
        await queueActor.ResubmitAllAsync();

        var jobsActor = BackgroundJobsPageActor.Create(this);

        Assert.True(jobsActor.HasWorkerItem("Resubmit all"));
    }

    [Fact]
    public async Task ResubmitAllQueue_Should_ShowScheduledState()
    {
        var queueActor = DetailPageActor.Create(this, QueueName);
        await queueActor.ResubmitAllAsync();

        var jobsActor = BackgroundJobsPageActor.Create(this);

        Assert.True(jobsActor.HasState(WorkItemState.Scheduled.ToString()));
    }

    // ===== Subscription ResubmitAll =====

    [Fact]
    public async Task ResubmitAllSubscription_Should_CreateBackgroundJob()
    {
        var subActor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        await subActor.ResubmitAllAsync();

        var jobsActor = BackgroundJobsPageActor.Create(this);

        Assert.True(jobsActor.HasWorkerItem("Resubmit all"));
    }

    [Fact]
    public async Task ResubmitAllSubscription_Should_ShowScheduledState()
    {
        var subActor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        await subActor.ResubmitAllAsync();

        var jobsActor = BackgroundJobsPageActor.Create(this);

        Assert.True(jobsActor.HasState(WorkItemState.Scheduled.ToString()));
    }

    // ===== Entity info =====

    [Fact]
    public async Task DeleteAllQueue_Should_ShowEntityName()
    {
        var queueActor = DetailPageActor.Create(this, QueueName);
        await queueActor.DeleteAllActiveAsync();

        var jobsActor = BackgroundJobsPageActor.Create(this);

        Assert.True(jobsActor.ContainsText(QueueName));
    }
}
