namespace Droomploeg.Postbode.Application.ServiceBus.Factories;

/// <summary>
/// IAdapter factory interface.
/// </summary>
/// <typeparam name="T">Adapter</typeparam>
public interface IAdapterFactory<T> where T : notnull
{
    /// <summary>
    /// Create adapter.
    /// </summary>
    /// <param name="mode"><see cref="AdapterMode"/></param>
    /// <returns>Adapter</returns>
    T Create(AdapterMode mode);
}
