using Droomploeg.DreamOps.Domain.Workers.Types;

namespace Droomploeg.DreamOps.Domain.Workers.Models;

/// <summary>
/// Worker item.
/// </summary>public class WorkItem
public class WorkerItem
{
    private readonly Func<CancellationToken, Task> _action;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly List<WorkerAction> _actions = [];
    private readonly List<WorkerEvent> _events = [];

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="userName">Name of the user creating the work item</param>
    /// <param name="entity">Name of the entity</param>
    /// <param name="description">Description</param>
    /// <param name="workerAction"><see cref="Action{T1, T2}"> with <see cref="Task"/> action</param>
    public WorkerItem(string userName, string entity, string description, Func<CancellationToken, Task> workerAction)
    {
        _action = workerAction;
        UserName = string.IsNullOrWhiteSpace(userName)
            ? "Anonymous"
            : userName;
        Entity = entity;
        Description = description;

        var createAction = new WorkerAction(userName, WorkItemAction.Schedule);
        _actions.Add(createAction);
        var createdEvent = new WorkerEvent(WorkItemState.Scheduled);
        _events.Add(createdEvent);
    }

    /// <summary>
    /// Identifier of the work item.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// User name who created the work item.
    /// </summary>
    public string UserName { get; } = "Anonymous";

    /// <summary>
    /// Entity.
    /// </summary>
    public string Entity { get; }

    /// <summary>
    /// Description of the work item.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// State of the work item.
    /// </summary>
    public WorkItemState State => _events.Last().State;

    /// <summary>
    /// <see cref="DateTimeOffset"/> of the last update of the work item."/>
    /// </summary>
    public DateTimeOffset UpdatedTimestamp => _events.Last().Timestamp;

    /// <summary>
    /// Indication if the work item can be cancelled.
    /// </summary>
    /// <returns></returns>
    public bool CanBeCancelled()
        => !_cancellationTokenSource.IsCancellationRequested &&
           (
            _events.Last().State == WorkItemState.Scheduled ||
            _events.Last().State == WorkItemState.Started 
           );

    public bool IsCancelled
        => _cancellationTokenSource.IsCancellationRequested;
    
    /// <summary>
    /// Cancel the work item.
    /// </summary>
    public async Task CancelAsync(string userName)
    {
        _actions.Add(new WorkerAction(userName, WorkItemAction.Cancel));
        await _cancellationTokenSource.CancelAsync();
    }

    /// <summary>
    /// Execute the work item.
    /// </summary>
    /// <returns><see cref="Task"/></returns>
    public async Task ExecuteAsync()
    {
        try
        {
            var startAction = new WorkerAction(UserName, WorkItemAction.Start);
            _actions.Add(startAction);
            var startEvent = new WorkerEvent(WorkItemState.Started);
            _events.Add(startEvent);

            await _action.Invoke(_cancellationTokenSource.Token);

            var completedEvent = new WorkerEvent(WorkItemState.Completed);
            _events.Add(completedEvent);
        }
        catch (OperationCanceledException)
        {
            var cancelledEvent = new WorkerEvent(WorkItemState.Cancelled);
            _events.Add(cancelledEvent);
        }
        catch (Exception exception)
        {
            var failedEvent = new WorkerEvent(WorkItemState.Failed, exception);
            _events.Add(failedEvent);
            throw;
        }
    }

    /// <summary>
    /// List of <see cref="WorkerAction"/> performed on the work item.
    /// </summary>
    public IReadOnlyCollection<WorkerAction> Actions => _actions.AsReadOnly();

    /// <summary>
    /// List of <see cref="WorkerEvent"/> occurred on the work item.
    /// </summary>
    public IReadOnlyCollection<WorkerEvent> Events => _events.AsReadOnly();
}
