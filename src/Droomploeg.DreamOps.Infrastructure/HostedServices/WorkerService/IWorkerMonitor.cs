namespace Droomploeg.DreamOps.Infrastructure.HostedServices.WorkerService
{
    public interface IWorkerMonitor
    {
        public static readonly WorkItemState[] FinishedStates = [WorkItemState.Completed, WorkItemState.Cancelled, WorkItemState.Failed];

        IEnumerable<WorkItem> GetWorkItems();
        IEnumerable<WorkItem> GetUpdatedWorkItems(DateTimeOffset lastCheckTime);
        void RegisterWorkItem(WorkItem item);
        void Unregister(Guid id);
        void UnregisterAllFinishedWorkItems();
    }
}