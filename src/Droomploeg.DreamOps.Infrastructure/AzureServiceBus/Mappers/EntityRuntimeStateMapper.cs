using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Mappers;

/// <summary>
/// Maps Azure Service Bus <see cref="EntityStatus"/> to domain <see cref="EntityRuntimeState"/> model.
/// </summary>
[ExcludeFromCodeCoverage( Justification = "Mapper class")]
internal static class EntityRuntimeStateMapper
{
    /// <summary>
    /// Maps a <see cref="EntityStatus"/> to a <see cref="EntityRuntimeState"/>.
    /// </summary>
    /// <param name="sbStatus">The Azure Service Bus entity status.</param>
    /// <returns>The corresponding <see cref="EntityRuntimeState"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status is not recognized.</exception>
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
