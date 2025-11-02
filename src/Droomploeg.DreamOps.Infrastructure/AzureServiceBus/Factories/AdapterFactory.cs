using Droomploeg.DreamOps.Application.ServiceBus.Factories;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Droomploeg.DreamOps.Infrastructure.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Factories;

/// <summary>
/// Adapater factory implementation.
/// </summary>
/// <typeparam name="T">Adapter</typeparam>
public class AdapterFactory<T> : IAdapterFactory<T> where T : notnull
{
    private readonly ApplicationContext _context;
    private readonly IServiceProvider _provider;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="provider"><see cref="IServiceProvider"/></param>
    /// <param name="context"><see cref="ApplicationContext"/></param>
    /// <exception cref="ArgumentNullException">When provider of context are null</exception>
    public AdapterFactory(
        IServiceProvider provider,
        ApplicationContext context)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc cref="IAdapterFactory{T}.Create(AdapterMode)"/>
    public T Create(AdapterMode mode)
    {
        if (mode == AdapterMode.OnBehalfOf)
        {
            return _provider.GetRequiredService<T>();
        }

        using var scope = _provider.CreateScope();
        var scopedContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        scopedContext.CorrelationId = _context.CorrelationId;
        scopedContext.CurrentConnection = _context.CurrentConnection;
        scopedContext.CurrentConnectionType = ServiceBusConnectionType.ServiceAccount;
        scopedContext.UserName = _context.UserName;

        return scope.ServiceProvider.GetRequiredService<T>();
    }
}
