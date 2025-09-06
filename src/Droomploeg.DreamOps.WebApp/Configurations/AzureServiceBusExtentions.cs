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
        var credentialOptions = new DefaultAzureCredentialOptions
        {
            ManagedIdentityClientId = configuration["Azure_Client_Id"],
        };
        var defaultCredentials = new DefaultAzureCredential(credentialOptions);


        var connectionList = configuration.GetSection(AzureServiceBusConnection.SectionName).Get<List<AzureServiceBusConnection>>() ?? [];
        var clientManager = new ServiceBusClientManager(connectionList.Select(c => c.Name));
        foreach (var connection in connectionList)
        {
            services.RegisterAzureServiceBusConnections(defaultCredentials, connection);
        }
        services.AddTransient<ApplicationInsightsLink>();
        services.AddSingleton(clientManager);
        services.AddScoped<IServiceBusClientContext, DefaultServiceBusClientContext>();

        return services;
    }

    public static IServiceCollection RegisterAzureServiceBusConnections(this IServiceCollection services, DefaultAzureCredential defaultCredentials, AzureServiceBusConnection connection)
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

        if (connection.EnableBackgroundService)
        {
            services.AddAzureClients(builder =>
            {
                builder.AddServiceBusClientWithNamespace(connection.FullyQualifiedNamespace)
                    .WithName($"Service_{connection.Name}")
                    .WithCredential(defaultCredentials);
                builder.AddServiceBusAdministrationClientWithNamespace(connection.FullyQualifiedNamespace)
                    .WithName($"Service_{connection.Name}")
                    .WithCredential(defaultCredentials);
            });
        }

        return services;
    }
}
