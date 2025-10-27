namespace Droomploeg.DreamOps.Domain.ServiceBus.Types;

public readonly record struct ServiceBusConnection(string Name)
{ 
    public string ClientName(ServiceBusConnectionType type)
        => ClientName(Name, type);

    public static string ClientName(string name, ServiceBusConnectionType type)
        => $"{name}-{type}".ToLower();

    public bool IsNotDefined => string.IsNullOrWhiteSpace(Name);

    public static ServiceBusConnection Undefined => new(string.Empty);
}
