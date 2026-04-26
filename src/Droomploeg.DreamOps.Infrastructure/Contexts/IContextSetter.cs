
namespace Droomploeg.DreamOps.Infrastructure.Contexts;

/// <summary>
/// Context setter interface.
/// </summary>
public interface IContextSetter
{
    /// <summary>
    /// Get and update the application context.
    /// </summary>
    /// <returns><see cref="ApplicationContext"/></returns>
    Task<ApplicationContext> GetAndUpdateAsync();
}
