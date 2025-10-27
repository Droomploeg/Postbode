using Azure.Identity;
using Droomploeg.DreamOps.Application.ServiceBus.Services;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Services;
using Droomploeg.DreamOps.WebApp.Common;
using Droomploeg.DreamOps.WebApp.Configurations.Options;
using Droomploeg.DreamOps.WebApp.Security;
using Microsoft.Extensions.Azure;

namespace Droomploeg.DreamOps.WebApp.Configurations;

internal static class AzureServiceBusExtentions
{
    internal static IServiceCollection AddAzureServiceBus(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var managedIdentityClientId = configuration["ManagedIdentityClientId"];
        var configConnectionList = configuration.GetSection(AzureServiceBusConnection.SectionName).Get<List<AzureServiceBusConnection>>() ?? [];
        var defaultCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            ManagedIdentityClientId = managedIdentityClientId
        });

        foreach (var connection in configConnectionList)
        {
            services.RegisterAzureUserAccountConnections(connection);
            if (IsServiceAccountEnabled(managedIdentityClientId, connection.EnableBackgroundService))
            {
                services.RegisterAzureServiceAccountConnections(connection, defaultCredential);
            }
        }

        var connectionList = configConnectionList
            .Select(c => new ServiceBusConnectionInfo
            (
                new ServiceBusConnection(c.Name),
                IsServiceAccountEnabled(managedIdentityClientId, c.EnableBackgroundService)
                    ? [ServiceBusConnectionType.UserAccount, ServiceBusConnectionType.ServiceAccount]
                    : [ServiceBusConnectionType.UserAccount])
            );

        var connectionService = new ConnectionService(connectionList);
        services
            .AddSingleton<IConnectionService>(connectionService)
            .AddTransient<ApplicationInsightsLink>();

        return services;
    }

    private static IServiceCollection RegisterAzureUserAccountConnections(this IServiceCollection services, AzureServiceBusConnection connection)
    {
        services.AddAzureClients(builder =>
        {
            var name = ServiceBusConnection.ClientName(connection.Name, ServiceBusConnectionType.UserAccount);

            builder.AddServiceBusClientWithNamespace(connection.FullyQualifiedNamespace)
                .WithName(name)
                .WithCredential(sp => new OnBehalfOfTokenCredential(sp, [OnBehalfOfTokenCredential.ServiceBusScope]));
            builder.AddServiceBusAdministrationClientWithNamespace(connection.FullyQualifiedNamespace)
                .WithName(name)
                .WithCredential(sp => new OnBehalfOfTokenCredential(sp, [OnBehalfOfTokenCredential.ServiceBusScope]));
        });

        return services;
    }

    private static IServiceCollection RegisterAzureServiceAccountConnections(this IServiceCollection services, AzureServiceBusConnection connection, DefaultAzureCredential credential)
    {
        services.AddAzureClients(builder =>
        {
            var name = ServiceBusConnection.ClientName(connection.Name, ServiceBusConnectionType.ServiceAccount);

            builder.AddServiceBusClientWithNamespace(connection.FullyQualifiedNamespace)
                .WithName(name)
                .WithCredential(credential);
            builder.AddServiceBusAdministrationClientWithNamespace(connection.FullyQualifiedNamespace)
                .WithName(name)
                .WithCredential(credential);
        });

        return services;
    }

    private static bool IsServiceAccountEnabled(string? managedIdentityClientId, bool enableBackgroundService)
        => !string.IsNullOrWhiteSpace(managedIdentityClientId) && enableBackgroundService;

}
