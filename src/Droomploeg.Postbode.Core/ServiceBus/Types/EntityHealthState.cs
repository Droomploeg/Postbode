namespace Droomploeg.Postbode.Domain.ServiceBus.Types;

/// <summary>
/// Entity Health State.
/// </summary>
public enum EntityHealthState
{
    /// <summary>
    /// Unknown state.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// No messages in the entity.
    /// </summary>
    NoMessages,

    /// <summary>
    /// Has active messages.
    /// </summary>
    HasActiveMessages,

    /// <summary>
    /// Transfer message count exceeded threshold.
    /// </summary>
    TransferMessageCount,

    /// <summary>   
    /// Has scheduled messages.
    /// </summary>
    HasScheduledMessages,

    /// <summary>
    /// Has dead-letter messages.    
    /// </summary>
    HasDeadLetterMessages,
}
