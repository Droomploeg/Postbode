namespace Droomploeg.DreamOps.Domain.ServiceBus.Types;

/// <summary>
/// Message source.
/// </summary>
public enum MessageSource
{
    /// <summary>
    /// Dead letter message source.
    /// </summary>
    DeadLetterMessage = 0,

    /// <summary>
    /// Active message source.
    /// </summary>
    ActiveMessage = 1
}
