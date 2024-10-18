namespace Droomploeg.DreamOps.Infrastructure.HostedServices.WorkerService;

public enum WorkItemState
{
    Created,
    Processing,
    Completed,
    Failed,
    Cancelled
}
