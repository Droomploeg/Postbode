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
    /// Created state.
    /// </summary>
    Created,

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
