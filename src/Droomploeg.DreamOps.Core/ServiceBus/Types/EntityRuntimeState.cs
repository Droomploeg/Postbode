namespace Droomploeg.DreamOps.Domain.ServiceBus.Types;

/// <summary>
/// Entity Runtime State.
/// </summary>
public enum EntityRuntimeState
{
    /// <summary>
    /// Unknown state.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Active.
    /// </summary>
    Active,

    /// <summary>
    /// Disabled.
    /// </summary>
    Disabled,

    /// <summary>
    /// Send disabled.
    /// </summary>
    SendDisabled,

    /// <summary>
    /// Receive disabled.    
    /// </summary>
    ReceiveDisabled
}
