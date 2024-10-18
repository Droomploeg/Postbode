namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus;

public class ServiceBusClientManager(IEnumerable<string> clients)
{
    public string[] Clients { get; } = clients.ToArray() ?? [];

    public string? Current { get; set; }
}
