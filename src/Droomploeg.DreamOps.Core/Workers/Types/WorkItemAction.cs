namespace Droomploeg.DreamOps.Domain.Workers.Types;

/// <summary>
/// Work item action.
/// </summary>
public enum WorkItemAction
{
    /// <summary>
    /// Create.
    /// </summary>
    Create,

    /// <summary>
    /// Start.
    /// </summary>
    Start,

    /// <summary>
    /// Finished.
    /// </summary>
    Finished,

    /// <summary>
    /// Cancel.
    /// </summary>
    Cancel,
}
