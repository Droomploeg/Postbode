namespace Droomploeg.Postbode.Domain.ServiceBus.Types;

/// <summary>
/// Service bus connection.
/// </summary>
/// <param name="Name"></param>
public readonly record struct ServiceBusConnection(string Name)
{
    /// <summary>
    /// Client name for the connection and type.
    /// </summary>
    /// <param name="type"><see cref="ServiceBusConnectionType"/></param>
    /// <returns>Name</returns>
    public string ClientName(ServiceBusConnectionType type)
        => ClientName(Name, type);

    /// <summary>
    /// Client name for the connection name and type.
    /// </summary>
    /// <param name="name"><see cref="string"/> name</param>
    /// <param name="type"><see cref="ServiceBusConnectionType"/></param>
    /// <returns>Name</returns>
    public static string ClientName(string name, ServiceBusConnectionType type)
        => $"{name}-{type}".ToLower();

    /// <summary>
    /// <see langword="true"/> when <see cref="ServiceBusConnection"/> is not defined.
    /// </summary>
    public bool IsNotDefined => string.IsNullOrWhiteSpace(Name);

    /// <summary>
    /// Create a <see cref="ServiceBusConnection"/> undefined.
    /// </summary>
    public static ServiceBusConnection Undefined => new(string.Empty);
}
