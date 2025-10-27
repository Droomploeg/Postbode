using Droomploeg.DreamOps.Application.Common;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Droomploeg.DreamOps.Infrastructure.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Dispatchers;

public class ManagedIdentityDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _provider;
    private readonly ApplicationContext _context;
    private readonly ILogger<ManagedIdentityDispatcher> _logger;

    public ManagedIdentityDispatcher(
        IServiceProvider provider,
        ApplicationContext context,
        ILogger<ManagedIdentityDispatcher> logger)
    {
        _provider = provider;
        _context = context;
        _logger = logger;
    }

    public async Task<bool> SendAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("SendAsync {Command} started", command);

        try
        {
            if (_context.CurrentConnection.IsNotDefined)
            {
                _logger.LogWarning("No connection found");
                return false;
            }

            using var scope = _provider.CreateScope();
            var scopedContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            scopedContext.CorrelationId = _context.CorrelationId;
            scopedContext.CurrentConnection = _context.CurrentConnection;
            scopedContext.CurrentConnectionType = ServiceBusConnectionType.ServiceAccount;

            var commandType = command.GetType();
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(commandType);
            var handler = scope.ServiceProvider.GetRequiredKeyedService(handlerType, CommandDispatcherFactory.ManagedIdentity);

            var method = handlerType.GetMethod("HandleAsync");
            var task = method!.Invoke(handler, [command, cancellationToken]) as Task;

            await task!;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to send command {Command}", command);
            return false;
        }

        _logger.LogDebug("SendAsync {Command} completed", command);
        return true;
    }
}
