namespace Droomploeg.DreamOps.Infrastructure.HostedServices.WorkerService
{
    /// <summary>
    /// Worker monitor interface.
    /// </summary>
    public interface IWorkerMonitor
    {
        /// <summary>
        /// Finished states of <see cref="WorkItem">.
        /// </summary>
        public static readonly WorkItemState[] FinishedStates = [WorkItemState.Completed, WorkItemState.Cancelled, WorkItemState.Failed];

        /// <summary>
        /// Get <see cref="WorkItem">.
        /// </summary>
        /// <returns><see cref="IEnumerable{T}"/> of <see cref="WorkItem"></returns>
        IEnumerable<WorkItem> GetWorkItems();

        /// <summary>
        /// Get <see cref="WorkItem"> that are updated before <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <param name="lastCheckTime">Last check <see cref="DateTimeOffset"/></param>
        /// <returns><see cref="IEnumerable{T}"/> of <see cref="WorkItem"></returns>
        IEnumerable<WorkItem> GetUpdatedWorkItems(DateTimeOffset lastCheckTime);

        /// <summary>
        /// Register <see cref="WorkItem">.
        /// </summary>
        /// <param name="item"><see cref="WorkItem"></param>
        void RegisterWorkItem(WorkItem item);

        /// <summary>
        /// Unregister <see cref="WorkItem">.
        /// </summary>
        /// <param name="id"><see cref="Guid"/> of the <see cref="WorkItem"></param>
        void Unregister(Guid id);

        /// <summary>
        /// Unregister all <see cref="WorkItem"> that are finished.
        /// </summary>
        void UnregisterAllFinishedWorkItems();
    }
}
