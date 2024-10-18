namespace Droomploeg.DreamOps.Infrastructure.HostedServices.WorkerService;

/// <summary>
/// Task queue interface.
/// </summary>
public interface IWorkerService
{
    /// <summary>
    /// Register work item.
    /// </summary>
    /// <param name="workItem"><see cref="WorkItem"/></param>
    void Register(WorkItem workItem);

    /// <summary>
    /// Execute next work item.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param> 
    /// <returns><see cref="Task"/></returns>
    Task ExecuteNextAsync(CancellationToken cancellationToken);
}
