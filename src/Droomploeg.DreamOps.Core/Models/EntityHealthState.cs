namespace Droomploeg.DreamOps.Core.Models;

/// <summary>
/// Entity Health State.
/// </summary>
public enum EntityHealthState
{
    Unknown = 0,
    NoMessages,
    HasScheduledMessages,
    HasActiveMessages,
    HasDeadLetterMessages,
}
