using Droomploeg.DreamOps.Domain.ServiceBus.Types;

namespace Droomploeg.DreamOps.Infrastructure.Contexts;

/// <summary>
/// Application context.
/// </summary>
public class ApplicationContext
{
    /// <summary>
    /// Correlation identifier.
    /// </summary>
    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User name.
    /// </summary>
    public string UserName { get; set; } = "Anonymous";

    /// <summary>
    /// Current Service Bus connection.
    /// </summary>
    public ServiceBusConnection CurrentConnection { get; set; } = ServiceBusConnection.Undefined;

    /// <summary>
    /// Current Service Bus connection type.
    /// </summary>
    public ServiceBusConnectionType CurrentConnectionType { get; set; } = ServiceBusConnectionType.UserAccount;
}
