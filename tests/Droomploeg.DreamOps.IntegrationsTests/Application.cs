using Bunit;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus;
using Droomploeg.DreamOps.IntegrationsTests.Common.Configurations;
using Droomploeg.DreamOps.IntegrationsTests.Common.ServiceBus;
using Droomploeg.DreamOps.WebApp.Configurations;
using Droomploeg.DreamOps.WebApp.Configurations.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Droomploeg.DreamOps.IntegrationsTests;

internal static class Application
{
    internal static async Task<ServiceBusConnection> SetupAsync(TestContext context)
    {
        var configuration = TestConfiguration.Configuration();

        context.Services.AddSingleton(configuration);
        context.Services.AddApplicationCore();
        context.Services.AddWorkerHostedServices();
        context.Services.AddAzureServiceBus(configuration);
        context.Services.AddTransient<QueueTestContext>();


        var connectionList = configuration.GetSection(AzureServiceBusConnection.SectionName).Get<List<AzureServiceBusConnection>>() ?? [];
        var connection = new ServiceBusConnection(connectionList[0].Name, connectionList[0].FullyQualifiedNamespace, false);

        await context.Services.GetRequiredService<IServiceBusConnectionAccessor>().SetCurrentAsync(connection);

        return connection;
    }
}
