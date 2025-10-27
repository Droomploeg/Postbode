using Droomploeg.DreamOps.Domain.Workers.Models;

namespace Droomploeg.DreamOps.Application.Workers.Services;

public interface IWorkerService
{
    bool Add(WorkerItem item);
    bool Cancel(Guid id);
    Task<bool> ExecuteNextAsync(CancellationToken cancellationToken);
    bool Remove(Guid id);

    IReadOnlyList<WorkerItem> GetAll();
}
