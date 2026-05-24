using Droomploeg.Postbode.Domain.ServiceBus.Types;

namespace Droomploeg.Postbode.Domain.ServiceBus.Models;

/// <summary>
/// Entity interface.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Name of the entity.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// <see cref="EntityRuntimeState"/>
    /// </summary>
    EntityRuntimeState RuntimeState { get; }

    /// <summary>
    /// <see cref="EntityHealthState"/>
    /// </summary>
    EntityHealthState HealthState { get; }
}
