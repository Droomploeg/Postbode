namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus;

/// <summary>
/// Service bus client context to hold the service bus current client information.
/// </summary>
public interface IServiceBusInfoContext
{
    /// <summary>
    /// Current servicebus information.
    /// </summary>
    ServiceBusInfo Current { get; set; }
}
