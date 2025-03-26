using Azure.Identity;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus;
using Droomploeg.DreamOps.WebApp.Common;
using Droomploeg.DreamOps.WebApp.Configurations.Options;
using Microsoft.Extensions.Azure;

namespace Droomploeg.DreamOps.WebApp.Configurations;

public static class AzureServiceBusExtentions
{
    public static IServiceCollection AddAzureServiceBus(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionList = configuration.GetSection(AzureServiceBusConnection.SectionName).Get<List<AzureServiceBusConnection>>() ?? [];
        var clientManager = new ServiceBusClientManager(connectionList.Select(c => c.Name));
        foreach (var connection in connectionList)
        {
            services.RegisterAzureServiceBusConnections(configuration, connection);
        }
        services.AddTransient<ApplicationInsightsLink>();
        services.AddSingleton(clientManager);
        services.AddScoped<IServiceBusClientContext, DefaultServiceBusClientContext>();

        return services;
    }

    public static IServiceCollection RegisterAzureServiceBusConnections(this IServiceCollection services, IConfiguration configuration, AzureServiceBusConnection connection)
    {
        var credentialOptions = new DefaultAzureCredentialOptions
        {
            ManagedIdentityClientId = configuration["Azure_Client_Id"],
        };

        if (!string.IsNullOrWhiteSpace(connection.ConnectionString))
        {
            services.AddAzureClients(builder =>
            {
                builder.AddServiceBusClient(connection.ConnectionString)
                    .WithName(connection.Name);
                builder.AddServiceBusAdministrationClient(connection.ConnectionString)
                    .WithName(connection.Name);
                builder
                    .UseCredential(new DefaultAzureCredential(credentialOptions));
            });
        }
        else
        {
            services.AddAzureClients(builder =>
            {
                builder.AddServiceBusClientWithNamespace(connection.FullyQualifiedNamespace)
                    .WithName(connection.Name);
                builder.AddServiceBusAdministrationClientWithNamespace(connection.FullyQualifiedNamespace)
                    .WithName(connection.Name);
                builder
                    .UseCredential(new DefaultAzureCredential(credentialOptions));
            });
        }
        return services;
    }
}
