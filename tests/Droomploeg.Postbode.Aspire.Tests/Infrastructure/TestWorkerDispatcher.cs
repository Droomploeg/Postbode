using Droomploeg.Postbode.Application.Workers.Dispatcher;
using Droomploeg.Postbode.Application.Workers.Services;
using Droomploeg.Postbode.Domain.Workers.Models;

namespace Droomploeg.Postbode.Aspire.Tests.Infrastructure;

/// <summary>
/// Test implementation of IWorkerDispatcher that captures dispatched work items for assertion
/// and forwards them to the IWorkerService so they appear on the BackgroundJobsPage.
/// </summary>
public class TestWorkerDispatcher : IWorkerDispatcher
{
    private readonly List<WorkerItem> _dispatched = [];
    private readonly Lock _sync = new();
    private IWorkerService? _workerService;

    public IReadOnlyList<WorkerItem> DispatchedItems
    {
        get
        {
            lock (_sync) return [.. _dispatched];
        }
    }

    public void SetWorkerService(IWorkerService workerService) =>
        _workerService = workerService;

    public void Dispatch(WorkerItem workItem)
    {
        lock (_sync)
        {
            _dispatched.Add(workItem);
        }
        _workerService?.Add(workItem);
    }

    public void Clear()
    {
        lock (_sync) _dispatched.Clear();
    }
}
