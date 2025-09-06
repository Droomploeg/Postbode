using Azure.Core;
using Azure.Identity;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus;
using Droomploeg.DreamOps.WebApp.Common;
using Droomploeg.DreamOps.WebApp.Configurations.Options;
using Droomploeg.DreamOps.WebApp.Security;
using Microsoft.Extensions.Azure;
using Microsoft.Identity.Web;

namespace Droomploeg.DreamOps.WebApp.Configurations;

public static class AzureServiceBusExtentions
{
    public static IServiceCollection AddAzureServiceBus(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionList = configuration.GetSection(AzureServiceBusConnection.SectionName).Get<List<AzureServiceBusConnection>>() ?? [];
        var clientManager = new ServiceBusClientManager(connectionList.Select(c => c.Name));
        foreach (var connection in connectionList)
        {
            services.RegisterAzureServiceBusConnections(connection);
        }
        services.AddTransient<ApplicationInsightsLink>();
        services.AddSingleton(clientManager);
        services.AddScoped<IServiceBusClientContext, DefaultServiceBusClientContext>();

        return services;
    }

    public static IServiceCollection RegisterAzureServiceBusConnections(this IServiceCollection services, AzureServiceBusConnection connection)
    {
        services.AddAzureClients(builder =>
        {
            builder.AddServiceBusClientWithNamespace(connection.FullyQualifiedNamespace)
                .WithName(connection.Name)
                .WithCredential(sp => new OnBehalfOfTokenCredential(sp, [OnBehalfOfTokenCredential.ServiceBusScope]));
            builder.AddServiceBusAdministrationClientWithNamespace(connection.FullyQualifiedNamespace)
                .WithName(connection.Name)
                .WithCredential(sp => new OnBehalfOfTokenCredential(sp, [OnBehalfOfTokenCredential.ServiceBusScope]));
        });
    
        return services;
    }
}
