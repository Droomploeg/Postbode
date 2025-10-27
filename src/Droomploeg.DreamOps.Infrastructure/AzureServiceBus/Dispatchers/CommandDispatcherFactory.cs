using Droomploeg.DreamOps.Application.Common;
using Droomploeg.DreamOps.Application.ServiceBus.Services;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Dispatchers;

/// <summary>
/// Command dispatcher factory class.
/// </summary>
/// <param name="provider"></param>
/// <param name="connectionService"></param>
public class CommandDispatcherFactory : ICommandDispatcherFactory
{
    public const string OnBehalfOf = nameof(OnBehalfOf);
    public const string ManagedIdentity = nameof(ManagedIdentity);

    private readonly IServiceProvider _provider;
    private readonly IConnectionService _connectionService;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="provider"><see cref="IServiceProvider"/></param>
    /// <param name="connectionService"><see cref="IConnectionService"/></param>
    /// <exception cref="ArgumentNullException">Occurs when a parameter is null</exception>
    public CommandDispatcherFactory(IServiceProvider provider, IConnectionService connectionService)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
    }

    /// <inheritdoc cref="ICommandDispatcherFactory.GetDispatcher(ServiceBusConnection)"/>
    /// <exception cref="DispatcherException">Occurs when no dispatch found with the correct connection</exception>
    public ICommandDispatcher GetDispatcher(ServiceBusConnection connection)
    {
        var serviceBusConnectionInfo = _connectionService.GetConnection(connection)
            ?? throw new DispatcherException($"No connection found with name '{connection}'");

        if (serviceBusConnectionInfo.HasServiceAccount)
        {
            return _provider.GetRequiredKeyedService<ICommandDispatcher>(ManagedIdentity);
        }
        if (serviceBusConnectionInfo.HasUserAccount)
        {
            return _provider.GetRequiredKeyedService<ICommandDispatcher>(OnBehalfOf);
        }

        throw new DispatcherException($"Connection '{connection}' does not have a valid identity configured.");
    }
}
