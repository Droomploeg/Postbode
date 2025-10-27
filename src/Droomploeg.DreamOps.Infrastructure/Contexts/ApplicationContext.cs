using Droomploeg.DreamOps.Domain.ServiceBus.Types;

namespace Droomploeg.DreamOps.Infrastructure.Contexts;

public class ApplicationContext
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();   
    public ServiceBusConnection CurrentConnection { get; set; } = ServiceBusConnection.Undefined;
    public ServiceBusConnectionType CurrentConnectionType { get; set; } = ServiceBusConnectionType.UserAccount;
}
