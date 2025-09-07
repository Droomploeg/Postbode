namespace Droomploeg.DreamOps.Infrastructure.HostedServices.WorkerService;

/// <summary>
/// States of a work item.
/// </summary>
public enum WorkItemState
{
    /// <summary>
    /// Created state.
    /// </summary>
    Created,

    /// <summary>
    /// Processing state.
    /// </summary>
    Processing,

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
