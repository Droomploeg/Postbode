namespace Droomploeg.Postbode.Domain.Workers.Types;

/// <summary>
/// Work item state.
/// </summary>
public enum WorkItemState
{
    /// <summary>
    /// Invalid state.
    /// </summary>
    Invalid = -1,

    /// <summary>
    /// Scheduled state.
    /// </summary>
    Scheduled = 0,

    /// <summary>
    /// Started state.
    /// </summary>
    Started = 1,

    /// <summary>
    /// Completed state.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Failed state.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Cancelled state.
    /// </summary>
    Cancelled = 4
}
