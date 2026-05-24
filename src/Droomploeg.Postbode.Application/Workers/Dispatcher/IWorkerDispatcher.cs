using Droomploeg.Postbode.Domain.Workers.Models;

namespace Droomploeg.Postbode.Application.Workers.Dispatcher;

/// <summary>
/// Worker dispatcher interface.
/// </summary>
public interface IWorkerDispatcher
{
    /// <summary>
    /// Dispatch work item
    /// </summary>
    /// <param name="workItem"><see cref="WorkerItem"/></param>
    public void Dispatch(WorkerItem workItem);
}
