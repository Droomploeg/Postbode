using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.Postbode.Domain.ServiceBus.Types;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Factories;

namespace Droomploeg.Postbode.Aspire.Tests.Infrastructure;

/// <summary>
/// Test double for the service account administration client provider. The emulator only
/// supports connection strings, so every connection maps to one client on the emulator.
/// </summary>
public sealed class TestServiceAccountAdminClientProvider : IServiceAccountAdminClientProvider
{
    private readonly ServiceBusAdministrationClient _client;

    public TestServiceAccountAdminClientProvider(string connectionString)
    {
        _client = new ServiceBusAdministrationClient(connectionString);
    }

    public ServiceBusAdministrationClient GetClient(ServiceBusConnection connection) => _client;
}
