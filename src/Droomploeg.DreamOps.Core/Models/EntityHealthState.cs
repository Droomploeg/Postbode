namespace Droomploeg.DreamOps.Core.Models;

/// <summary>
/// Entity Health State.
/// </summary>
public enum EntityHealthState
{
    NoMessages,
    HasScheduledMessages,
    HasActiveMessages,
    HasDeadLetterMessages,
}
