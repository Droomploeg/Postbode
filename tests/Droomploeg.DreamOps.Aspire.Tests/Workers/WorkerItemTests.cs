using Droomploeg.DreamOps.Domain.Workers.Models;
using Droomploeg.DreamOps.Domain.Workers.Types;

namespace Droomploeg.DreamOps.Aspire.Tests.Workers;

public class WorkerItemTests
{
    private static WorkerItem CreateItem(Func<CancellationToken, Task>? action = null)
        => new("test-user", Guid.NewGuid(), "test-queue", "Test description",
            action ?? (_ => Task.CompletedTask));

    // ===== Initial state =====

    [Fact]
    public void NewItem_Should_HaveScheduledState()
    {
        var item = CreateItem();

        Assert.Equal(WorkItemState.Scheduled, item.State);
    }

    [Fact]
    public void NewItem_Should_HaveOneScheduledEvent()
    {
        var item = CreateItem();

        Assert.Single(item.Events);
        Assert.Equal(WorkItemState.Scheduled, item.Events.First().State);
    }

    [Fact]
    public void NewItem_Should_HaveOneScheduleAction()
    {
        var item = CreateItem();

        Assert.Single(item.Actions);
        Assert.Equal(WorkItemAction.Schedule, item.Actions.First().Action);
    }

    [Fact]
    public void NewItem_Should_SetTimestamps()
    {
        var before = DateTimeOffset.UtcNow;
        var item = CreateItem();

        Assert.True(item.CreatedTimestamp >= before);
        Assert.Equal(item.CreatedTimestamp, item.UpdatedTimestamp);
    }

    [Fact]
    public void NewItem_Should_BeCancellable()
    {
        var item = CreateItem();

        Assert.True(item.CanBeCancelled());
        Assert.False(item.IsCancelled);
    }

    [Fact]
    public void NewItem_WithNullUsername_Should_UseAnonymous()
    {
        var item = new WorkerItem(null!, Guid.NewGuid(), "entity", "desc", _ => Task.CompletedTask);

        Assert.Equal("Anonymous", item.Username);
    }

    [Fact]
    public void NewItem_WithWhitespaceUsername_Should_UseAnonymous()
    {
        var item = new WorkerItem("  ", Guid.NewGuid(), "entity", "desc", _ => Task.CompletedTask);

        Assert.Equal("Anonymous", item.Username);
    }

    // ===== Execute — success =====

    [Fact]
    public async Task Execute_Should_TransitionToCompleted()
    {
        var item = CreateItem();

        await item.ExecuteAsync();

        Assert.Equal(WorkItemState.Completed, item.State);
    }

    [Fact]
    public async Task Execute_Should_RecordStartAndCompletedEvents()
    {
        var item = CreateItem();

        await item.ExecuteAsync();

        Assert.Equal(3, item.Events.Count);
        var events = item.Events.ToList();
        Assert.Equal(WorkItemState.Scheduled, events[0].State);
        Assert.Equal(WorkItemState.Started, events[1].State);
        Assert.Equal(WorkItemState.Completed, events[2].State);
    }

    [Fact]
    public async Task Execute_Should_RecordStartAction()
    {
        var item = CreateItem();

        await item.ExecuteAsync();

        Assert.Equal(2, item.Actions.Count);
        var actions = item.Actions.ToList();
        Assert.Equal(WorkItemAction.Schedule, actions[0].Action);
        Assert.Equal(WorkItemAction.Start, actions[1].Action);
    }

    [Fact]
    public async Task Execute_Should_InvokeAction()
    {
        var invoked = false;
        var item = CreateItem(_ =>
        {
            invoked = true;
            return Task.CompletedTask;
        });

        await item.ExecuteAsync();

        Assert.True(invoked);
    }

    [Fact]
    public async Task Execute_Completed_Should_NotBeCancellable()
    {
        var item = CreateItem();

        await item.ExecuteAsync();

        Assert.False(item.CanBeCancelled());
    }

    // ===== Execute — failure =====

    [Fact]
    public async Task Execute_WhenActionThrows_Should_TransitionToFailed()
    {
        var item = CreateItem(_ => throw new InvalidOperationException("test error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => item.ExecuteAsync());

        Assert.Equal(WorkItemState.Failed, item.State);
    }

    [Fact]
    public async Task Execute_WhenActionThrows_Should_RecordFailedEventWithException()
    {
        var item = CreateItem(_ => throw new InvalidOperationException("test error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => item.ExecuteAsync());

        var failedEvent = item.Events.Last();
        Assert.Equal(WorkItemState.Failed, failedEvent.State);
        Assert.IsType<InvalidOperationException>(failedEvent.Exception);
        Assert.Equal("test error", failedEvent.Exception!.Message);
    }

    [Fact]
    public async Task Execute_WhenActionThrows_Should_NotBeCancellable()
    {
        var item = CreateItem(_ => throw new InvalidOperationException("test error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => item.ExecuteAsync());

        Assert.False(item.CanBeCancelled());
    }

    // ===== Cancel — before execution =====

    [Fact]
    public async Task Cancel_BeforeExecution_Should_TransitionToCancelled()
    {
        var item = CreateItem();

        await item.CancelAsync("admin");

        Assert.Equal(WorkItemState.Cancelled, item.State);
    }

    [Fact]
    public async Task Cancel_BeforeExecution_Should_RecordCancelAction()
    {
        var item = CreateItem();

        await item.CancelAsync("admin");

        var cancelAction = item.Actions.Last();
        Assert.Equal(WorkItemAction.Cancel, cancelAction.Action);
        Assert.Equal("admin", cancelAction.UserName);
    }

    [Fact]
    public async Task Cancel_BeforeExecution_Should_NotBeCancellableAgain()
    {
        var item = CreateItem();

        await item.CancelAsync("admin");

        Assert.False(item.CanBeCancelled());
    }

    // ===== Cancel — during execution =====

    [Fact]
    public async Task Cancel_DuringExecution_Should_RequestCancellationToken()
    {
        var tcs = new TaskCompletionSource();
        CancellationToken capturedToken = default;

        var item = CreateItem(async ct =>
        {
            capturedToken = ct;
            tcs.SetResult();
            await Task.Delay(Timeout.Infinite, ct);
        });

        var executeTask = item.ExecuteAsync();
        await tcs.Task;

        Assert.Equal(WorkItemState.Started, item.State);
        Assert.True(item.CanBeCancelled());

        await item.CancelAsync("admin");

        Assert.True(capturedToken.IsCancellationRequested);

        await executeTask;

        Assert.Equal(WorkItemState.Cancelled, item.State);
    }

    // ===== GetEventDateTime =====

    [Fact]
    public void GetEventDateTime_Should_ReturnTimestampForState()
    {
        var item = CreateItem();

        var scheduledTime = item.GetEventDateTime(WorkItemState.Scheduled);

        Assert.NotEqual(default, scheduledTime);
    }

    [Fact]
    public void GetEventDateTime_ForNonExistentState_Should_ReturnDefault()
    {
        var item = CreateItem();

        var completedTime = item.GetEventDateTime(WorkItemState.Completed);

        Assert.Equal(default, completedTime);
    }
}
