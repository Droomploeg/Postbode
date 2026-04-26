using Droomploeg.DreamOps.Domain.Workers.Models;
using Droomploeg.DreamOps.Domain.Workers.Types;
using Droomploeg.DreamOps.Infrastructure.Workers.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Droomploeg.DreamOps.Aspire.Tests.Workers;

public class WorkerServiceTests
{
    private readonly WorkerService _service = new(NullLogger<WorkerService>.Instance);

    private static WorkerItem CreateItem(Func<CancellationToken, Task>? action = null)
        => new("test-user", Guid.NewGuid(), "test-queue", "Test",
            action ?? (_ => Task.CompletedTask));

    // ===== Add =====

    [Fact]
    public void Add_Should_ReturnTrue()
    {
        var item = CreateItem();

        var result = _service.Add(item);

        Assert.True(result);
    }

    [Fact]
    public void Add_Should_AppearInGetAll()
    {
        var item = CreateItem();

        _service.Add(item);

        Assert.Single(_service.GetAll());
        Assert.Same(item, _service.GetAll()[0]);
    }

    [Fact]
    public void Add_Multiple_Should_AppearInGetAll()
    {
        _service.Add(CreateItem());
        _service.Add(CreateItem());
        _service.Add(CreateItem());

        Assert.Equal(3, _service.GetAll().Count);
    }

    // ===== Remove =====

    [Fact]
    public void Remove_ScheduledItem_Should_ReturnTrue()
    {
        var item = CreateItem();
        _service.Add(item);

        var result = _service.Remove(item.Id);

        Assert.True(result);
        Assert.Empty(_service.GetAll());
    }

    [Fact]
    public void Remove_NonExistentId_Should_ReturnFalse()
    {
        var result = _service.Remove(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task Remove_StartedItem_Should_ReturnFalse()
    {
        var tcs = new TaskCompletionSource();
        var item = CreateItem(async _ =>
        {
            tcs.SetResult();
            await Task.Delay(Timeout.Infinite, CancellationToken.None);
        });
        _service.Add(item);

        var executeTask = _service.ExecuteNextAsync(CancellationToken.None);
        await tcs.Task;

        var result = _service.Remove(item.Id);

        Assert.False(result);
        Assert.Single(_service.GetAll());
    }

    [Fact]
    public async Task Remove_CompletedItem_Should_ReturnTrue()
    {
        var item = CreateItem();
        _service.Add(item);
        await _service.ExecuteNextAsync(CancellationToken.None);

        var result = _service.Remove(item.Id);

        Assert.True(result);
        Assert.Empty(_service.GetAll());
    }

    [Fact]
    public async Task Remove_CancelledItem_Should_ReturnTrue()
    {
        var item = CreateItem();
        _service.Add(item);
        await item.CancelAsync("admin");

        var result = _service.Remove(item.Id);

        Assert.True(result);
        Assert.Empty(_service.GetAll());
    }

    // ===== ExecuteNextAsync =====

    [Fact]
    public async Task ExecuteNext_WithNoItems_Should_ReturnFalse()
    {
        var result = await _service.ExecuteNextAsync(CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task ExecuteNext_WithScheduledItem_Should_ReturnTrue()
    {
        _service.Add(CreateItem());

        var result = await _service.ExecuteNextAsync(CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task ExecuteNext_Should_ExecuteOldestFirst()
    {
        var executionOrder = new List<string>();

        var item1 = new WorkerItem("user", Guid.NewGuid(), "queue", "first",
            _ => { executionOrder.Add("first"); return Task.CompletedTask; });
        var item2 = new WorkerItem("user", Guid.NewGuid(), "queue", "second",
            _ => { executionOrder.Add("second"); return Task.CompletedTask; });

        _service.Add(item1);
        _service.Add(item2);

        await _service.ExecuteNextAsync(CancellationToken.None);
        await _service.ExecuteNextAsync(CancellationToken.None);

        Assert.Equal(["first", "second"], executionOrder);
    }

    [Fact]
    public async Task ExecuteNext_Should_SkipCompletedItems()
    {
        var item1 = CreateItem();
        var item2 = CreateItem();
        _service.Add(item1);
        _service.Add(item2);

        await _service.ExecuteNextAsync(CancellationToken.None);

        Assert.Equal(WorkItemState.Completed, item1.State);
        Assert.Equal(WorkItemState.Scheduled, item2.State);

        await _service.ExecuteNextAsync(CancellationToken.None);

        Assert.Equal(WorkItemState.Completed, item2.State);
    }

    [Fact]
    public async Task ExecuteNext_Should_SkipCancelledItems()
    {
        var item1 = CreateItem();
        var item2 = CreateItem();
        _service.Add(item1);
        _service.Add(item2);

        await item1.CancelAsync("admin");

        await _service.ExecuteNextAsync(CancellationToken.None);

        Assert.Equal(WorkItemState.Cancelled, item1.State);
        Assert.Equal(WorkItemState.Completed, item2.State);
    }

    [Fact]
    public async Task ExecuteNext_AllCompleted_Should_ReturnFalse()
    {
        _service.Add(CreateItem());
        await _service.ExecuteNextAsync(CancellationToken.None);

        var result = await _service.ExecuteNextAsync(CancellationToken.None);

        Assert.False(result);
    }
}
