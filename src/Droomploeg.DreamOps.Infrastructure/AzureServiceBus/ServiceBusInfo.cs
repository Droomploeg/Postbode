namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus;

/// <summary>
/// Service bus information
/// </summary>
/// <param name="Name">Name of the service bus</param>
/// <param name="BackgroundEnabled">Indication if background is enabled</param>
public record ServiceBusInfo(string Name, bool BackgroundEnabled)
{
    /// <summary>
    /// Background name.
    /// </summary>
    public string BackgroundName 
        => !string.IsNullOrWhiteSpace(Name) && BackgroundEnabled
            ? $"{Name}-background"
            : string.Empty;

    /// <summary>
    /// None instance.
    /// </summary>
    public static ServiceBusInfo None 
        => new (string.Empty, false);
}


