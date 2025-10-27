using Droomploeg.DreamOps.Domain.ServiceBus.Types;

namespace Droomploeg.DreamOps.Domain.ServiceBus.Models;

/// <summary>
/// Service bus connection
/// </summary>
/// <param name="Name">Name of the service bus</param>
/// <param name="ServiceAccountEnabled">Indication if service account is enabled</param>
public record ServiceBusConnectionInfo(ServiceBusConnection Connection, ServiceBusConnectionType[] connectionTypes)
{
    /// <summary>
    /// User account is enabled.
    /// </summary>
    public bool HasUserAccount
        => !string.IsNullOrWhiteSpace(Connection.Name) && connectionTypes.Contains(ServiceBusConnectionType.UserAccount);
            

    /// <summary>
    /// Service account is enabled.
    /// </summary>
    public bool HasServiceAccount
        => !string.IsNullOrWhiteSpace(Connection.Name) && connectionTypes.Contains(ServiceBusConnectionType.ServiceAccount);

    /// <summary>
    /// Undefined connection.
    /// </summary>
    public static ServiceBusConnectionInfo Undefined 
        => new (ServiceBusConnection.Undefined, []);
}


