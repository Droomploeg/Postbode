using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Droomploeg.DreamOps.Infrastructure.HostedServices.WorkerService;

public class WorkerMonitor(ILogger<WorkerMonitor> logger) : IWorkerMonitor
{
    private readonly ILogger<WorkerMonitor> _logger = logger;
    private readonly ConcurrentDictionary<Guid, WorkItem> _items = [];

    public IEnumerable<WorkItem> GetUpdatedWorkItems(DateTimeOffset lastCheckTime)
    {
        return _items.Values
            .Where(wi => wi.Timestamp > lastCheckTime)
            .OrderByDescending(_workItems => _workItems.Timestamp);
    }

    public IEnumerable<WorkItem> GetWorkItems()
    {
        return _items.Values
            .OrderByDescending(_workItems => _workItems.Timestamp);
    }

    public void RegisterWorkItem(WorkItem item)
    {
        try
        {
            _items.TryAdd(item.Id, item);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error occurred registering work item.");
        }
    }

    public void Unregister(Guid id)
    {
        try
        {
            _items.TryRemove(id, out _);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error occurred unregistering work item.");
        }
    }

    public void UnregisterAllFinishedWorkItems()
    {
        var closeItemIds = _items.Values
            .Where(wi => IWorkerMonitor.FinishedStates.Contains(wi.State))
            .Select(wi => wi.Id)
            .ToList();

        foreach (var id in closeItemIds)
        {
            try
            {
                if (!_items.TryRemove(id, out _))
                {
                    _logger.LogWarning("Unable to unregister work item, possible already removed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error occurred unregistering work item.");
            }
        }
    }
}
