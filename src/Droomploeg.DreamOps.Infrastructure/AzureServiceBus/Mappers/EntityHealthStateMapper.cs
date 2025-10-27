using Droomploeg.DreamOps.Domain.ServiceBus.Types;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Mappers;

internal static class EntityHealthStateMapper
{
    internal static EntityHealthState Map(
        long numberOfActiveMessage,
        long numberOfScheduledMessages,
        long numberOfTransferMessages,
        long numberOfDeadLetterMessages)
    {
        if (numberOfDeadLetterMessages > 0)
        {
            return EntityHealthState.HasDeadLetterMessages;
        }
        if (numberOfScheduledMessages > 0)
        {
            return EntityHealthState.HasScheduledMessages;
        }
        if (numberOfTransferMessages > 0)
        {
            return EntityHealthState.TransferMessageCount;
        }
        if (numberOfActiveMessage > 0)
        {
            return EntityHealthState.HasActiveMessages;
        }

        return EntityHealthState.NoMessages;
    }
}
