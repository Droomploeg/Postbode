using Droomploeg.DreamOps.Core.Models;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Mappers;

internal static class EntityHealthStateMapper
{
    internal static EntityHealthState Map(long numberOfScheduledMessages, long numberOfActiveMessage, long numberOfDeadLetterMessages)
    {
        if (numberOfDeadLetterMessages > 0)
        {
            return  EntityHealthState.HasDeadLetterMessages;
        }
        if (numberOfActiveMessage > 0)
        {
            return EntityHealthState.HasActiveMessages;
        }
        if (numberOfScheduledMessages > 0)
        { 
            return EntityHealthState.HasScheduledMessages;
        }

        return EntityHealthState.NoMessages;
    }
}
