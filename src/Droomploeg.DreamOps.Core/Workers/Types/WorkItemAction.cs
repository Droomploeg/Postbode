namespace Droomploeg.DreamOps.Domain.Workers.Types;

/// <summary>
/// Work item action.
/// </summary>
public enum WorkItemAction
{
    /// <summary>
    /// Create.
    /// </summary>
    Schedule,

    /// <summary>
    /// Start.
    /// </summary>
    Start,

    /// <summary>
    /// Cancel.
    /// </summary>
    Cancel,
}
