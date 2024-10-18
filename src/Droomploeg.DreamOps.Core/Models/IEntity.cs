namespace Droomploeg.DreamOps.Core.Models;

public interface IEntity
{
    string Name { get; }
    EntityRuntimeState RuntimeState { get; }

    EntityHealthState HealthState { get; }
}
