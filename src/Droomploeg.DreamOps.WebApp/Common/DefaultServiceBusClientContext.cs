using Droomploeg.DreamOps.Infrastructure.AzureServiceBus;

namespace Droomploeg.DreamOps.WebApp.Common;

public class DefaultServiceBusClientContext : IServiceBusClientContext
{
    public string CurrentClient { get; set; }
}
