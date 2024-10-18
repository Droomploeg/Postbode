namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus;

public interface IServiceBusClientContext
{
    string CurrentClient { get; set; }
}
