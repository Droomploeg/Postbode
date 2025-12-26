using Droomploeg.DreamOps.Application.Workers.Services;
using Droomploeg.DreamOps.Domain.Workers.Models;
using Droomploeg.DreamOps.Domain.Workers.Types;
using Microsoft.Extensions.Logging;

namespace Droomploeg.DreamOps.Infrastructure.Workers.Services;

/// <summary>
/// Worker service to manage background work items.
/// </summary>
public class WorkerService : IWorkerService
{
    //private readonly ILogger<WorkerService> _logger;
    private readonly List<WorkerItem> _workItemList = [];
    private readonly Lock _sync = new();

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="logger"><see cref="ILogger{TCategoryName}"/></param>
    /// <exception cref="ArgumentNullException"><see cref="ArgumentNullException"/></exception>
    public WorkerService()
    {
        //_logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc cref="IWorkerService.Add(WorkerItem)"/>>
    public bool Add(WorkerItem item)
    {
        //_logger.LogInformation("Added work item {WorkItemId} of type {WorkItemType}", item.Id, item.GetType().Name);
        lock (_sync)
        {
            _workItemList.Add(item);
        }

        return true;
    }

    /// <inheritdoc cref="IWorkerService.Remove(Guid)"/>
    public bool Remove(Guid id)
    {
        //_logger.LogInformation("Removing work item {WorkItemId}", id);
        var item = _workItemList.FirstOrDefault(i => i.Id == id);
        if (item is null)
        {
            return false;
        }

        if (item.State == WorkItemState.Started)
        {
            //_logger.LogWarning("Cannot remove work item {WorkItemId} because it is in state {WorkItemState}", id, item?.State ?? WorkItemState.Invalid);
            return false;
        }

        lock (_sync)
        {
            _workItemList.Remove(item);
        }
        return true;
    }

    /// <inheritdoc cref="IWorkerService.GetAll"/>
    public IReadOnlyList<WorkerItem> GetAll()
        => _workItemList.AsReadOnly();


    /// <inheritdoc cref="IWorkerService.ExecuteNextAsync(CancellationToken)"/>>
    public async Task<bool> ExecuteNextAsync(CancellationToken cancellationToken)
    {
        //_logger.LogInformation("Starting next work item");
        var nextWorkItem = _workItemList
                .Where(i => i.State == WorkItemState.Scheduled)
                .OrderBy(i => i.UpdatedTimestamp)
                .FirstOrDefault();

        if (nextWorkItem == null)
        {
            //_logger.LogInformation("No pending work items found");
            return false;
        }

        await nextWorkItem.ExecuteAsync();

        //_logger.LogInformation("Starting work item {WorkItemId} of type {WorkItemType}", nextWorkItem.Id, nextWorkItem.GetType().Name);

        return true;
    }
}
