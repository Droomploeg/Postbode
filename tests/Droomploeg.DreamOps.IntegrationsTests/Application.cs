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
    internal static void Setup(TestContext context, out ServiceBusInfo serviceBusInfo)
    {
        var configuration = TestConfiguration.Configuration();

        context.Services.AddSingleton(configuration);
        context.Services.AddApplicationCore();
        context.Services.AddWorkerHostedServices();
        context.Services.AddAzureServiceBus(configuration);
        context.Services.AddTransient<QueueTestContext>();


        var connectionList = configuration.GetSection(AzureServiceBusConnection.SectionName).Get<List<AzureServiceBusConnection>>() ?? [];
        serviceBusInfo = new ServiceBusInfo(connectionList[0].Name, false);

        context.Services.GetRequiredService<IServiceBusInfoContext>().Current = serviceBusInfo;
    }
}
