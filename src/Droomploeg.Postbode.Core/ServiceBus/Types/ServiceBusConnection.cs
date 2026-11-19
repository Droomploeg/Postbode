namespace Droomploeg.Postbode.Domain.ServiceBus.Types;

/// <summary>
/// Service bus connection.
/// </summary>
/// <param name="Name"></param>
public readonly record struct ServiceBusConnection(string Name)
{
    /// <summary>
    /// <see langword="true"/> when <see cref="ServiceBusConnection"/> is not defined.
    /// </summary>
    public bool IsNotDefined => string.IsNullOrWhiteSpace(Name);

    /// <summary>
    /// Create a <see cref="ServiceBusConnection"/> undefined.
    /// </summary>
    public static ServiceBusConnection Undefined => new(string.Empty);
}
