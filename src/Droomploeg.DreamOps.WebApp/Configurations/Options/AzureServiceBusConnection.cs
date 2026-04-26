using System.Diagnostics.CodeAnalysis;

namespace Droomploeg.DreamOps.WebApp.Configurations.Options;

[ExcludeFromCodeCoverage( Justification = "Options class")]
public class AzureServiceBusConnection
{
    public const string SectionName = "AzureServiceBusConnections";

    public string Name { get; set; } = string.Empty;
    public string FullyQualifiedNamespace { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public bool EnableBackgroundService { get; set; } = false;
}
