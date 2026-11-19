namespace Droomploeg.Postbode.Domain.ServiceBus.Types;

/// <summary>
/// Service bus connection type for clients registered through the Azure client factory.
/// User clients are deliberately not part of this: they are created per user by
/// <c>IUserServiceBusClientProvider</c>, so they are never shared between users.
/// </summary>
public enum ServiceBusConnectionType
{
    /// <summary>
    /// Service account connection.
    /// </summary>
    ServiceAccount,
}
