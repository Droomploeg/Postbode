using Droomploeg.DreamOps.Domain.Workers.Types;

namespace Droomploeg.DreamOps.Domain.Workers.Models;

/// <summary>
/// Represents a single unit of work that can be executed, cancelled and tracked.
/// </summary>
public class WorkerItem
{
    private readonly Func<CancellationToken, Task> _action;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly List<WorkerAction> _actions = [];
    private readonly List<WorkerEvent> _events = [];

    /// <summary>
    /// Initializes a new instance of <see cref="WorkerItem"/>.
    /// </summary>
    /// <param name="userName">Name of the user creating the work item. If null or whitespace, "Anonymous" will be used.</param>
    /// <param name="entity">Name of the entity the work item relates to.</param>
    /// <param name="description">Human readable description of the work item.</param>
    /// <param name="workerAction">The asynchronous action that will be executed for this work item. Receives a <see cref="CancellationToken"/>.</param>
    public WorkerItem(string userName, string entity, string description, Func<CancellationToken, Task> workerAction)
    {
        _action = workerAction;
        Username = string.IsNullOrWhiteSpace(userName)
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
    /// Gets the identifier of the work item.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the username who created the work item.
    /// </summary>
    public string Username { get; } = "Anonymous";

    /// <summary>
    /// Gets the entity associated with this work item.
    /// </summary>
    public string Entity { get; }

    /// <summary>
    /// Gets the description of the work item.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the current state of the work item (determined from the latest event).
    /// </summary>
    public WorkItemState State => _events.Last().State;

    /// <summary>
    /// Gets the <see cref="DateTimeOffset"/> of the creation of the work item.
    /// </summary>
    public DateTimeOffset CreatedTimestamp => _events.First().Timestamp;    
    
    /// <summary>
    /// Gets the <see cref="DateTimeOffset"/> of the last update to the work item.
    /// </summary>
    public DateTimeOffset UpdatedTimestamp => _events.Last().Timestamp;

    /// <summary>
    /// Determines whether the work item can be cancelled at this time.
    /// Returns true when the cancellation has not been requested and the current state
    /// is either <see cref="WorkItemState.Scheduled"/> or <see cref="WorkItemState.Started"/>.
    /// </summary>
    public bool CanBeCancelled()
        => !_cancellationTokenSource.IsCancellationRequested &&
           (
            _events.Last().State == WorkItemState.Scheduled ||
            _events.Last().State == WorkItemState.Started
           );

    /// <summary>
    /// Indicates whether cancellation has been requested for this work item.
    /// </summary>
    public bool IsCancelled
        => _cancellationTokenSource.IsCancellationRequested;

    /// <summary>
    /// Requests cancellation of the work item and records the cancel action.
    /// This method is asynchronous only for API symmetry; cancellation is requested synchronously.
    /// </summary>
    /// <param name="username">Name of the user requesting cancellation.</param>
    /// <returns>A completed <see cref="Task"/> once the cancellation request has been recorded.</returns>
    public Task CancelAsync(string username)
    {
        _actions.Add(new WorkerAction(username, WorkItemAction.Cancel));
        if (_events.Last().State == WorkItemState.Started)
        {
            _cancellationTokenSource.Cancel();
        }
        else
        {
            var cancelledEvent = new WorkerEvent(WorkItemState.Cancelled);
            _events.Add(cancelledEvent);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes the work item's action. Records start, completion, cancellation, and failure events.
    /// Exceptions thrown by the action are recorded as a failed event and then rethrown.
    /// </summary>
    /// <returns>A task that represents the asynchronous execute operation.</returns>
    /// <exception cref="Exception">If the underlying action throws, the exception will be rethrown after recording a failed event.</exception>
    public async Task ExecuteAsync()
    {
        try
        {
            var startAction = new WorkerAction(Username, WorkItemAction.Start);
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
    /// Get <see cref="DateTimeOffset" /> of <see cref="WorkItemState"/>.
    /// </summary>
    /// <param name="state"><see cref="WorkItemState"/></param>
    /// <returns><see cref="DateTimeOffset"/></returns>
    public DateTimeOffset GetEventDateTime(WorkItemState state)
        => _events.Where(e => e.State == state)
            .Select(e => e.Timestamp)
            .FirstOrDefault();
    
    /// <summary>
    /// Gets a read-only collection of actions performed on the work item.
    /// </summary>
    public IReadOnlyCollection<WorkerAction> Actions => _actions.AsReadOnly();

    /// <summary>
    /// Gets a read-only collection of events that have occurred on the work item.
    /// </summary>
    public IReadOnlyCollection<WorkerEvent> Events => _events.AsReadOnly();
}
