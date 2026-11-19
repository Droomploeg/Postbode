using Azure.Messaging.ServiceBus;
using Droomploeg.Postbode.Domain.ServiceBus.Types;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Exceptions;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Factories;
using Droomploeg.Postbode.Infrastructure.Contexts;
using Microsoft.Identity.Web;

namespace Droomploeg.Postbode.WebApp.Security;

/// <summary>
/// Scoped provider of <see cref="ServiceBusClient"/> instances for the user in the
/// <see cref="ApplicationContext"/>. Each Blazor circuit, and each background job, gets its own
/// instance, and therefore its own AMQP connections and token cache, so a token acquired for one
/// user is never used for another. Clients are created lazily and disposed with the scope.
/// </summary>
internal sealed partial class UserServiceBusClientProvider : IUserServiceBusClientProvider, IAsyncDisposable, IDisposable
{
    private readonly IReadOnlyDictionary<string, string> _namespaces;
    private readonly ApplicationContext _context;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly ILogger<UserServiceBusClientProvider> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Dictionary<string, ServiceBusClient> _clients = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _sync = new();
    private bool _disposed;

    /// <param name="namespaces">Fully qualified namespace per connection name.</param>
    /// <param name="context">Holds the user of the current circuit or background job.</param>
    /// <param name="tokenAcquisition">Acquires delegated tokens for that user.</param>
    /// <param name="logger">Logger for this provider.</param>
    /// <param name="loggerFactory">Creates the loggers for the credentials this provider creates.</param>
    public UserServiceBusClientProvider(
        IReadOnlyDictionary<string, string> namespaces,
        ApplicationContext context,
        ITokenAcquisition tokenAcquisition,
        ILogger<UserServiceBusClientProvider> logger,
        ILoggerFactory loggerFactory)
    {
        _namespaces = namespaces ?? throw new ArgumentNullException(nameof(namespaces));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _tokenAcquisition = tokenAcquisition ?? throw new ArgumentNullException(nameof(tokenAcquisition));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public async ValueTask<ServiceBusClient> GetClientAsync(ServiceBusConnection connection, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_clients.TryGetValue(connection.Name, out var existingClient))
            {
                return existingClient;
            }
        }

        if (!_namespaces.TryGetValue(connection.Name, out var fullyQualifiedNamespace)
            || string.IsNullOrWhiteSpace(fullyQualifiedNamespace))
        {
            LogUnknownConnection(connection.Name);
            throw new InvalidServiceBusConnectionException();
        }

        var user = _context.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            LogUnauthenticated(connection.Name);
            throw new UnauthorizedAccessException("A Service Bus client for a user requires an authenticated user.");
        }

        var credential = new UserTokenCredential(_tokenAcquisition, user, _loggerFactory.CreateLogger<UserTokenCredential>());
        var client = new ServiceBusClient(fullyQualifiedNamespace, credential);

        ServiceBusClient? otherClient = null;
        var added = false;
        lock (_sync)
        {
            if (!_disposed && !_clients.TryGetValue(connection.Name, out otherClient))
            {
                _clients[connection.Name] = client;
                added = true;
            }
        }

        if (!added)
        {
            // Another call created the client first, or the circuit ended in the meantime.
            await client.DisposeAsync();
            return otherClient ?? throw new ObjectDisposedException(nameof(UserServiceBusClientProvider));
        }

        LogClientCreated(connection.Name);
        return client;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var client in TakeClientsForDisposal())
        {
            await client.DisposeAsync();
        }
    }

    public void Dispose()
    {
        // A DI scope that is disposed synchronously still has to release the AMQP connections.
        foreach (var client in TakeClientsForDisposal())
        {
            client.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }

    private List<ServiceBusClient> TakeClientsForDisposal()
    {
        List<ServiceBusClient> clients;
        lock (_sync)
        {
            if (_disposed)
            {
                return [];
            }

            _disposed = true;
            clients = [.. _clients.Values];
            _clients.Clear();
        }

        if (clients.Count > 0)
        {
            LogDisposingClients(clients.Count);
        }

        return clients;
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Created user Service Bus client for connection {ConnectionName}")]
    private partial void LogClientCreated(string connectionName);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "No fully qualified namespace configured for connection {ConnectionName}")]
    private partial void LogUnknownConnection(string connectionName);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Refused user Service Bus client for connection {ConnectionName}: user is not authenticated")]
    private partial void LogUnauthenticated(string connectionName);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Disposing {ClientCount} user Service Bus client(s)")]
    private partial void LogDisposingClients(int clientCount);
}
