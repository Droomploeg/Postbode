namespace Droomploeg.DreamOps.Domain.ServiceBus.Types;

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
