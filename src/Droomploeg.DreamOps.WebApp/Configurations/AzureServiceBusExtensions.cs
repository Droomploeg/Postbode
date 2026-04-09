using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using Droomploeg.DreamOps.Application.ServiceBus.Services;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Services;
using Droomploeg.DreamOps.WebApp.Common;
using Droomploeg.DreamOps.WebApp.Configurations.Options;
using Droomploeg.DreamOps.WebApp.Security;
using Microsoft.Extensions.Azure;

namespace Droomploeg.DreamOps.WebApp.Configurations;

[ExcludeFromCodeCoverage( Justification = "Azure service bus configuration extensions")]
internal static class AzureServiceBusExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddAzureServiceBus(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            var managedIdentityClientId = configuration["ManagedIdentityClientId"];
            var configConnectionList = configuration
                .GetSection(AzureServiceBusConnection.SectionName)
                .Get<List<AzureServiceBusConnection>>() ?? []; 
        
            var defaultCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId = managedIdentityClientId
            });


            foreach (var connection in configConnectionList)
            {
                services
                    .RegisterAzureUserAccountConnections(connection)
                    .RegisterAzureServiceAccountConnections(connection, defaultCredential);
            }
            
            var connectionList = configConnectionList
                .Select(c => new ServiceBusConnection(c.Name));

            var connectionService = new ConnectionService(connectionList);
            services
                .AddSingleton<IConnectionService>(connectionService)
                .AddTransient<ApplicationInsightsLink>();

            return services;
        }

        private IServiceCollection RegisterAzureUserAccountConnections(AzureServiceBusConnection connection)
        {
            services.AddAzureClients(builder =>
            {
                var name = ServiceBusConnection.ClientName(connection.Name, ServiceBusConnectionType.UserAccount);

                builder.AddServiceBusClientWithNamespace(connection.FullyQualifiedNamespace)
                    .WithName(name)
                    .WithCredential(sp => new OnBehalfOfTokenCredential(sp, [OnBehalfOfTokenCredential.ServiceBusScope]));
            });

            return services;
        }

        private IServiceCollection RegisterAzureServiceAccountConnections(AzureServiceBusConnection connection, DefaultAzureCredential credential)
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
    }
}
