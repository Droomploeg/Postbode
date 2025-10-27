namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Exceptions;

/// <summary>
/// Occurs when dispatcher failed.
/// </summary>
public sealed class DispatcherException : Exception
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">Message</param>
    public DispatcherException(string message)
        : base(message)
    {
    }
}
