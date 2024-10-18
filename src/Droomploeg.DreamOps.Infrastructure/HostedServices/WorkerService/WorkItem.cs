namespace Droomploeg.DreamOps.Infrastructure.HostedServices.WorkerService;

public class WorkItem(string entity, string description, Func<CancellationToken, Task> action)
{
    private readonly Func<CancellationToken, Task> _action = action;
    private readonly CancellationTokenSource cancellationTokenSource = new();

    public Guid Id { get; } = Guid.NewGuid();

    public DateTimeOffset Timestamp { get; private set; } = DateTimeOffset.UtcNow;

    public WorkItemState State { get; private set; } = WorkItemState.Created;

    public string Entity { get; } = entity;
    public string Description { get; } = description;

    public void Cancel()
    {
        cancellationTokenSource.Cancel();
    }

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
