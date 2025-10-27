namespace Droomploeg.DreamOps.Domain.ServiceBus.Types;

/// <summary>
/// Entity Health State.
/// </summary>
public enum EntityHealthState
{
    Unknown = 0,
    NoMessages,
    HasActiveMessages,
    TransferMessageCount,
    HasScheduledMessages,
    HasDeadLetterMessages,
}
