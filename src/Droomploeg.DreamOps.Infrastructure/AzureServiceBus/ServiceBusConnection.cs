namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus;

/// <summary>
/// Service bus connection
/// </summary>
/// <param name="Name">Name of the service bus</param>
/// <param name="FullyQualifiedNamespace">Fully qualified namespace of the service bus</param>
/// <param name="BackgroundServiceEnabled">Indication if background is enabled</param>
public record ServiceBusConnection(string Name, string FullyQualifiedNamespace, bool BackgroundServiceEnabled)
{
    /// <summary>
    /// Background name.
    /// </summary>
    public string BackgroundName 
        => !string.IsNullOrWhiteSpace(Name) && BackgroundServiceEnabled
            ? $"{Name}-background"
            : string.Empty;

    /// <summary>
    /// None instance.
    /// </summary>
    public static ServiceBusConnection None 
        => new (string.Empty, string.Empty, false);
}


