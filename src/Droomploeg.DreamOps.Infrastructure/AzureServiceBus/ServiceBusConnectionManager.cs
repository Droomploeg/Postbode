namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus;

/// <summary>
/// Service bus connection manager
/// </summary>
/// <param name="serviceBusInformationList"></param>
public class ServiceBusConnectionManager(IEnumerable<ServiceBusConnection> serviceBusInformationList)
{

    /// <summary>
    /// Service bus information list.
    /// </summary>
    public ServiceBusConnection[] ServiceBusInformationList { get; } = serviceBusInformationList.ToArray() ?? [];

    /// <summary>
    /// Current service bus information.
    /// </summary>
    public ServiceBusConnection? Current { get; set; }
}
