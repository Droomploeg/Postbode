using Azure.Messaging.ServiceBus;
using Droomploeg.Postbode.Domain.ServiceBus.Types;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Factories;

namespace Droomploeg.Postbode.Aspire.Tests.Infrastructure;

/// <summary>
/// Test double for the per-user client provider. The emulator only supports connection strings,
/// so every connection maps to one client on the emulator. Like the real provider it is scoped,
/// creates its client lazily and supports both synchronous and asynchronous disposal.
/// </summary>
public sealed class TestUserServiceBusClientProvider : IUserServiceBusClientProvider, IAsyncDisposable, IDisposable
{
    private readonly Lazy<ServiceBusClient> _client;

    public TestUserServiceBusClientProvider(string connectionString)
    {
        _client = new Lazy<ServiceBusClient>(() => new ServiceBusClient(connectionString));
    }

    public ValueTask<ServiceBusClient> GetClientAsync(ServiceBusConnection connection, CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(_client.Value);

    public async ValueTask DisposeAsync()
    {
        if (_client.IsValueCreated)
        {
            await _client.Value.DisposeAsync();
        }
    }

    public void Dispose()
    {
        if (_client.IsValueCreated)
        {
            _client.Value.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }
}
