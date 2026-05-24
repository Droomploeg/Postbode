using System.Diagnostics.CodeAnalysis;

namespace Droomploeg.Postbode.Infrastructure.AzureServiceBus.Exceptions;

/// <summary>
/// Invalid service bus connection exception class.
/// </summary>
[ExcludeFromCodeCoverage( Justification = "Mapper class")]
public sealed class InvalidServiceBusConnectionException : Exception
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public InvalidServiceBusConnectionException()
        : base("The current Service Bus connection is not defined.")
    {
    }

    /// <summary>
    /// Throw if clientName is null or white space.
    /// </summary>
    /// <param name="clientName">ClientName</param>
    /// <exception cref="InvalidServiceBusConnectionException">Occurs when client is null or white space</exception>
    public static void ThrowIfNullOrWhiteSpace(string clientName)
    {
        if (string.IsNullOrWhiteSpace(clientName))
        {
            throw new InvalidServiceBusConnectionException();
        }
    }
}
