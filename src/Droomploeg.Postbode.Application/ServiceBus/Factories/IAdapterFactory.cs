namespace Droomploeg.Postbode.Application.ServiceBus.Factories;

/// <summary>
/// IAdapter factory interface. Every adapter acts with the delegated token of the signed-in user.
/// </summary>
/// <typeparam name="T">Adapter</typeparam>
public interface IAdapterFactory<T> where T : notnull
{
    /// <summary>
    /// Create an adapter for an action that runs directly within the current request or circuit.
    /// </summary>
    /// <returns>Adapter</returns>
    T Create();

    /// <summary>
    /// Create the action of a background job. The current user and context are captured now; when
    /// the job runs, it gets its own adapter, with its own Service Bus client for that user, which is
    /// released when the job ends. The job therefore keeps running after the circuit has ended.
    /// </summary>
    /// <param name="work">The work to run with the adapter.</param>
    /// <returns>The action to hand to the background job.</returns>
    Func<CancellationToken, Task> CreateBackgroundAction(Func<T, CancellationToken, Task> work);
}
