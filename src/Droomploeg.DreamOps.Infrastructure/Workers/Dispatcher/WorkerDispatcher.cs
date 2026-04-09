using System.Diagnostics.CodeAnalysis;
using Droomploeg.DreamOps.Application.Workers.Dispatcher;
using Droomploeg.DreamOps.Application.Workers.Services;
using Droomploeg.DreamOps.Domain.Workers.Models;

namespace Droomploeg.DreamOps.Infrastructure.Workers.Dispatcher;

/// <summary>
/// Worker dispatcher implementation.
/// </summary>
[ExcludeFromCodeCoverage( Justification = "Testable via TestWorkerDispatcher")] 
public class WorkerDispatcher : IWorkerDispatcher
{
    private readonly IWorkerService _service;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="service"><see cref="IWorkerService"/></param>
    /// <exception cref="ArgumentNullException">Occurs when service is null</exception>
    public WorkerDispatcher(IWorkerService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    /// <inheritdoc cref="IWorkerDispatcher.Dispatch(WorkerItem)"/>
    public void Dispatch(WorkerItem workItem)
    {
        _service.Add(workItem);
    }
}
