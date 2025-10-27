using Droomploeg.DreamOps.Domain.ServiceBus.Types;

namespace Droomploeg.DreamOps.WebApp.Common;

internal static class EntityStateHelper
{
    internal static string GetCssClass(EntityRuntimeState runtimeState, EntityHealthState healthState)
    {
        var runtimeStateCss = runtimeState switch
        {
            EntityRuntimeState.Disabled => "disabled",
            EntityRuntimeState.SendDisabled => "nosend",
            EntityRuntimeState.ReceiveDisabled => "noreceive",
            _ => "active",
        };

        var healthStateCss = healthState switch
        {
            EntityHealthState.HasDeadLetterMessages => "deadlettermessages",
            EntityHealthState.HasActiveMessages => "activemessages",
            EntityHealthState.HasScheduledMessages => "scheduledmessages",
            _ => "nomessages"
        };

        return $"entity-state-{healthStateCss}-{runtimeStateCss}";
    }

    internal static string GetHelpTitle(EntityRuntimeState runtimeState, EntityHealthState healthState)
    {
        var runtimeStateMessage = runtimeState switch
        {
            EntityRuntimeState.Disabled => "Disabled",
            EntityRuntimeState.SendDisabled => "Send disabled",
            EntityRuntimeState.ReceiveDisabled => "Receive disabled",
            _ => "Active",
        };

        var healthStateMessage = healthState switch
        {
            EntityHealthState.HasDeadLetterMessages => "dead-letter messages",
            EntityHealthState.HasActiveMessages => "active messages",
            EntityHealthState.HasScheduledMessages => "scheduled messages",
            _ => "no messages"
        };

        return $"{runtimeStateMessage} with {healthStateMessage}";
    }
}
