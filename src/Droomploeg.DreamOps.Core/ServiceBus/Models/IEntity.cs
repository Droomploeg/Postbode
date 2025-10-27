using Droomploeg.DreamOps.Domain.ServiceBus.Types;

namespace Droomploeg.DreamOps.Domain.ServiceBus.Models;

public interface IEntity
{
    string Name { get; }
    EntityRuntimeState RuntimeState { get; }

    EntityHealthState HealthState { get; }
}
