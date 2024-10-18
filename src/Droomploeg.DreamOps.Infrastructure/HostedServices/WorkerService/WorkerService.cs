using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Droomploeg.DreamOps.Infrastructure.HostedServices.WorkerService;

/// <summary>
/// Task queue implementation.
/// </summary>
public class WorkerService(IWorkerMonitor monitor, ILogger<WorkerService> logger) : IWorkerService
{
    private readonly IWorkerMonitor _monitor = monitor;
    private readonly ILogger<WorkerService> _logger = logger;
    private readonly ConcurrentQueue<WorkItem> _queue = new();
    //private readonly Channel<WorkItem> _queue = Channel.CreateUnbounded<WorkItem>();


    /// <inheritdoc/>>
    public void Register(WorkItem workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        _queue.Enqueue(workItem);
        _monitor.RegisterWorkItem(workItem);
        //if (_queue.Writer.TryWrite(workItem))
        //{
        //    _monitor.RegisterWorkItem(workItem);
        //}
        //else 
        //{ 
        //    _logger.LogError("Unable to register work item, possible already removed");
        //}
    }

    /// <inheritdoc/>>
    public async Task ExecuteNextAsync(CancellationToken cancellationToken)
    {
        if (_queue.TryDequeue(out var workItem))
        //if (_queue.Reader.TryRead(out var workItem))
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
