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

        var managedIdentityClientId = configuration["ManagedIdentityClientId"];
        var configConnectionList = configuration.GetSection(AzureServiceBusConnection.SectionName).Get<List<AzureServiceBusConnection>>() ?? [];
        var defaultCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            ManagedIdentityClientId = managedIdentityClientId
        });

        var connectionList = configConnectionList
            .Select(c => new ServiceBusConnection
            (
                c.Name,
                c.FullyQualifiedNamespace,
                c.EnableBackgroundService && !string.IsNullOrWhiteSpace(managedIdentityClientId)
            ));

        foreach (var connection in connectionList)
        {
            services.RegisterAzureServiceBusUserConnections(connection);
            if (connection.BackgroundServiceEnabled)
            {
                services.RegisterAzureServiceBusBackgroundConnections(connection, defaultCredential);
            }
        }

        var serviceBusConnectionManager = new ServiceBusConnectionManager(connectionList);
        services.AddSingleton(serviceBusConnectionManager);
        services.AddTransient<ApplicationInsightsLink>();
        services.AddScoped<IServiceBusConnectionAccessor, ServiceBusConnectionAccessor>();

        return services;
    }

    public static IServiceCollection RegisterAzureServiceBusUserConnections(this IServiceCollection services, ServiceBusConnection connection)
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

    public static IServiceCollection RegisterAzureServiceBusBackgroundConnections(this IServiceCollection services, ServiceBusConnection connection, DefaultAzureCredential credential)
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
