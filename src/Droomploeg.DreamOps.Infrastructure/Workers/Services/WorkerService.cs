using Droomploeg.DreamOps.Application.Workers.Services;
using Droomploeg.DreamOps.Domain.Workers.Models;
using Droomploeg.DreamOps.Domain.Workers.Types;
using Microsoft.Extensions.Logging;

namespace Droomploeg.DreamOps.Infrastructure.Workers.Services;

/// <summary>
/// Worker service to manage background work items.
/// </summary>
//public class WorkerService(ILogger<WorkerService> logger) : IWorkerService
public class WorkerService() : IWorkerService
{
    //private readonly ILogger<WorkerService> _logger = logger;
    private readonly List<WorkerItem> _workItemList = [];

    private readonly Lock _sync = new();

    public bool Add(WorkerItem item)
    {
        //_logger.LogInformation("Added work item {WorkItemId} of type {WorkItemType}", item.Id, item.GetType().Name);

        lock (_sync)
        {
            _workItemList.Add(item);
        }

        return true;
    }

    public async Task<bool> ExecuteNextAsync(CancellationToken cancellationToken)
    {
        //_logger.LogInformation("Starting next work item");
        var nextWorkItem = _workItemList
                .Where(i => i.State == WorkItemState.Created)
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

    public bool Remove(Guid id)
    {
        //_logger.LogInformation("Removing work item {WorkItemId}", id);
        var item = _workItemList.FirstOrDefault(i => i.Id == id);
        if (item == null || !item.CanBeCancelled())
        {
            //_logger.LogWarning("Cannot remove work item {WorkItemId} because it is in state {WorkItemState}", id, item?.Info.State ?? WorkItemState.Invalid);
            return false;
        }

        lock (_sync)
        {
            _workItemList.Remove(item);
        }
        return true;
    }

    public IReadOnlyList<WorkerItem> GetAll()
        => _workItemList.AsReadOnly();
}
