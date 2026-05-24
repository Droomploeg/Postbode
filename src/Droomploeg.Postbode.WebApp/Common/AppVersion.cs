using System.Reflection;

namespace Droomploeg.Postbode.WebApp.Common;

/// <summary>
/// Provides the application version from the assembly metadata.
/// </summary>
public static class AppVersion
{
    /// <summary>
    /// Application version derived from the InformationalVersion assembly attribute.
    /// Set at build time via MSBuild properties or git tags.
    /// </summary>
    public static string Version { get; } =
        Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "unknown";
}
