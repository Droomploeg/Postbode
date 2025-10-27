using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Mappers;

internal static class EntityRuntimeStateMapper
{
    internal static EntityRuntimeState Map(EntityStatus sbStatus)
    {
        if (sbStatus == EntityStatus.Active)
        {
            return EntityRuntimeState.Active;
        }
        if (sbStatus == EntityStatus.Disabled)
        {
            return EntityRuntimeState.Disabled;
        }
        if (sbStatus == EntityStatus.SendDisabled)
        {
            return EntityRuntimeState.SendDisabled;
        }
        if (sbStatus == EntityStatus.ReceiveDisabled)
        {
            return EntityRuntimeState.ReceiveDisabled;
        }
        throw new ArgumentOutOfRangeException(nameof(sbStatus), sbStatus, null);
    }
}
