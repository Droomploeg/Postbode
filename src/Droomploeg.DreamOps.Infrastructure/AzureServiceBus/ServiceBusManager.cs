namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus;

/// <summary>
/// Service bus manager
/// </summary>
/// <param name="serviceBusInformationList"></param>
public class ServiceBusManager(IEnumerable<ServiceBusInfo> serviceBusInformationList)
{

    /// <summary>
    /// Service bus information list.
    /// </summary>
    public ServiceBusInfo[] ServiceBusInformationList { get; } = serviceBusInformationList.ToArray() ?? [];

    /// <summary>
    /// Current service bus information.
    /// </summary>
    public ServiceBusInfo? Current { get; set; }
}
