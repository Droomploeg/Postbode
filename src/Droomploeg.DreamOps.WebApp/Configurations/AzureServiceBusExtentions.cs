using Azure.Identity;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus;
using Droomploeg.DreamOps.WebApp.Common;
using Droomploeg.DreamOps.WebApp.Configurations.Options;
using Droomploeg.DreamOps.WebApp.Security;
using Microsoft.Extensions.Azure;

namespace Droomploeg.DreamOps.WebApp.Configurations;

public static class AzureServiceBusExtentions
{
    public static IServiceCollection AddAzureServiceBus(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionList = configuration.GetSection(AzureServiceBusConnection.SectionName).Get<List<AzureServiceBusConnection>>() ?? [];
        var clientManager = new ServiceBusManager(connectionList.Select(c => new ServiceBusInfo(c.Name, c.EnableBackgroundService)));
        foreach (var connection in connectionList)
        {
            services.RegisterAzureServiceBusUserConnections(connection);
        }

        var managedIdentityClientId = configuration["ManagedIdentityClientId"];
        if (!string.IsNullOrEmpty(managedIdentityClientId))
        {
            var defaultCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId = managedIdentityClientId
            });

            foreach (var connection in connectionList.Where(c => c.EnableBackgroundService))
            {
                services.RegisterAzureServiceBusBackgroundConnections(connection, defaultCredential);
            }
        }

        services.AddTransient<ApplicationInsightsLink>();
        services.AddSingleton(clientManager);
        services.AddScoped<IServiceBusInfoContext, DefaultServiceBusInfoContext>();

        return services;
    }

    public static IServiceCollection RegisterAzureServiceBusUserConnections(this IServiceCollection services, AzureServiceBusConnection connection)
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

    public static IServiceCollection RegisterAzureServiceBusBackgroundConnections(this IServiceCollection services, AzureServiceBusConnection connection, DefaultAzureCredential credential)
    {
        services.AddAzureClients(builder =>
        {
            builder.AddServiceBusClientWithNamespace(connection.FullyQualifiedNamespace)
                .WithName($"{connection.Name}-background")
                .WithCredential(credential);
            builder.AddServiceBusAdministrationClientWithNamespace(connection.FullyQualifiedNamespace)
                .WithName($"{connection.Name}-background")
                .WithCredential(credential);
        });

        return services;
    }
}
