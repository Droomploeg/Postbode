using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Droomploeg.DreamOps.Infrastructure.HostedServices.WorkerService;

/// <summary>
/// Task queue implementation.
/// </summary>
public class WorkerService(IWorkerMonitor monitor, ILogger<WorkerService> logger) : IWorkerService
{
    private readonly IWorkerMonitor _monitor = monitor;
    private readonly ILogger<WorkerService> _logger = logger;
    private readonly ConcurrentQueue<WorkItem> _queue = new();

    /// <inheritdoc cref="IWorkerService"/>
    public void Register(WorkItem workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        _queue.Enqueue(workItem);
        _monitor.RegisterWorkItem(workItem);
    }

    /// <inheritdoc cref="IWorkerService"/>
    public async Task ExecuteNextAsync(CancellationToken cancellationToken)
    {
        if (_queue.TryDequeue(out var workItem))
        {
            try
            {
                _logger.LogDebug("Executing {WorkItem}.", workItem);
                await workItem.ExecuteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing {WorkItem}.", workItem);
            }
        }
    }
}
