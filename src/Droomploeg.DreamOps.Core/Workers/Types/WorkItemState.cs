namespace Droomploeg.DreamOps.Domain.Workers.Types;

/// <summary>
/// Work item state.
/// </summary>
public enum WorkItemState
{
    /// <summary>
    /// Invalid state.
    /// </summary>
    Invalid,

    /// <summary>
    /// Scheduled state.
    /// </summary>
    Scheduled,

    /// <summary>
    /// Started state.
    /// </summary>
    Started,

    /// <summary>
    /// Completed state.
    /// </summary>
    Completed,

    /// <summary>
    /// Failed state.
    /// </summary>
    Failed,

    /// <summary>
    /// Cancelled state.
    /// </summary>
    Cancelled
}
