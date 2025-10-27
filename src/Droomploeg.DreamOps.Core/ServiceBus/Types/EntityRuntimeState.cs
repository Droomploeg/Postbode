namespace Droomploeg.DreamOps.Domain.ServiceBus.Types;

/// <summary>
/// Entity Runtime State.
/// </summary>
public enum EntityRuntimeState
{
    Unknown = 0,
    Active,
    Disabled,
    SendDisabled,
    ReceiveDisabled
}
