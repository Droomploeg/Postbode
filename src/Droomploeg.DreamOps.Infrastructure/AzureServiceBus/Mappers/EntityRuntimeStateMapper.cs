using ServiceBus = Azure.Messaging.ServiceBus.Administration;
using Model = Droomploeg.DreamOps.Core.Models;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Mappers;

internal static class EntityRuntimeStateMapper
{
    internal static Model.EntityRuntimeState Map(ServiceBus.EntityStatus sbStatus)
    {
        if (sbStatus == ServiceBus.EntityStatus.Active)
        {
            return Model.EntityRuntimeState.Active;
        }
        if (sbStatus == ServiceBus.EntityStatus.Disabled)
        {
            return Model.EntityRuntimeState.Disabled;
        }
        if (sbStatus == ServiceBus.EntityStatus.SendDisabled)
        {
            return Model.EntityRuntimeState.SendDisabled;
        }
        if (sbStatus == ServiceBus.EntityStatus.ReceiveDisabled)
        {
            return Model.EntityRuntimeState.ReceiveDisabled;
        }
        throw new ArgumentOutOfRangeException(nameof(sbStatus), sbStatus, null);
    }
}
