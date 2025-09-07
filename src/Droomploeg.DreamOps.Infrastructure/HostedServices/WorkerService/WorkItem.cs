namespace Droomploeg.DreamOps.Infrastructure.HostedServices.WorkerService;

/// <summary>
/// Worker item.
/// </summary>
/// <param name="entity">Name of the entity</param>
/// <param name="description">Description</param>
/// <param name="action"><see cref="Action{T1, T2}"> with <see cref="Task"/> action</param>
public class WorkItem(string entity, string description, Func<CancellationToken, Task> action)
{
    private readonly Func<CancellationToken, Task> _action = action;
    private readonly CancellationTokenSource cancellationTokenSource = new();

    /// <summary>
    /// Identifier of the work item.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp of the last state change.
    /// </summary>
    public DateTimeOffset Timestamp { get; private set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// State of the work item.
    /// </summary>
    public WorkItemState State { get; private set; } = WorkItemState.Created;

    /// <summary>
    /// Entity.
    /// </summary>
    public string Entity { get; } = entity;

    /// <summary>
    /// Description of the work item.
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    /// Cancel the work item.
    /// </summary>
    public void Cancel()
    {
        cancellationTokenSource.Cancel();
    }

    /// <summary>
    /// Execute the work item.
    /// </summary>
    /// <returns></returns>
    public async Task ExecuteAsync()
    {
        try
        {
            State = WorkItemState.Processing;
            Timestamp = DateTimeOffset.UtcNow;
            await _action.Invoke(cancellationTokenSource.Token);

            State = WorkItemState.Completed;
            Timestamp = DateTimeOffset.UtcNow;
        }
        catch (OperationCanceledException)
        {
            State = WorkItemState.Cancelled;
            Timestamp = DateTimeOffset.UtcNow;
        }
        catch (Exception)
        {
            State = WorkItemState.Failed;
            Timestamp = DateTimeOffset.UtcNow;
            throw;
        }
    }
}
