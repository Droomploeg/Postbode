using Droomploeg.Postbode.Domain.Workers.Models;

namespace Droomploeg.Postbode.Application.Workers.Services;

/// <summary>
/// Worker service interface.
/// </summary>
public interface IWorkerService
{
    /// <summary>
    /// Get all work items.
    /// </summary>
    /// <returns><see cref="IReadOnlyCollection{T}"/> of <see cref="WorkerItem"/></returns>
    IReadOnlyList<WorkerItem> GetAll();

    /// <summary>
    /// Adds the specified <see cref="WorkerItem"/> to the collection.
    /// </summary>
    /// <param name="item">The <see cref="WorkerItem"/> to add. Cannot be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="WorkerItem"/> was successfully added; otherwise, <see
    /// langword="false"/>.</returns>
    bool Add(WorkerItem item);

    /// <summary>
    /// Removes the specified <see cref="WorkerItem"/> from the collection by Id.
    /// </summary>
    /// <param name="id"><see cref="WorkerItem.Id"/></param>
    /// <returns><see langword="true"/> if the <see cref="WorkerItem"/> was successfully removed; otherwise, <see
    /// langword="false"/>.</returns>
    bool Remove(Guid id);

    /// <summary>
    /// Executes the next work item in the collection.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see langword="true"/> if the <see cref="WorkerItem"/> was successfully removed; otherwise, <see
    /// langword="false"/>.</returns>
    Task<bool> ExecuteNextAsync(CancellationToken cancellationToken);
}
