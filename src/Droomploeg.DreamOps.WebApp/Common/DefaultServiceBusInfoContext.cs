using Droomploeg.DreamOps.Infrastructure.AzureServiceBus;

namespace Droomploeg.DreamOps.WebApp.Common;

/// <summary>
/// Default implementation of <see cref="IServiceBusInfoContext"/>.
/// </summary>
public class DefaultServiceBusInfoContext : IServiceBusInfoContext
{
    /// <see cref="IServiceBusInfoContext.Current">
    public ServiceBusInfo Current { get; set; } = ServiceBusInfo.None;
}
