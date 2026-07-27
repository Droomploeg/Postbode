using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using Droomploeg.Postbode.Application.ServiceBus.Services;
using Droomploeg.Postbode.Domain.ServiceBus.Types;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Services;
using Droomploeg.Postbode.WebApp.Common;
using Droomploeg.Postbode.WebApp.Configurations.Options;
using Droomploeg.Postbode.WebApp.Security;
using Microsoft.Extensions.Azure;

namespace Droomploeg.Postbode.WebApp.Configurations;

[ExcludeFromCodeCoverage( Justification = "Azure service bus configuration extensions")]
internal static class AzureServiceBusExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddAzureServiceBus(IConfiguration configuration, IHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(environment);

            var managedIdentityClientId = configuration["ManagedIdentityClientId"];
            var configConnectionList = configuration
                .GetSection(AzureServiceBusConnection.SectionName)
                .Get<List<AzureServiceBusConnection>>() ?? [];

            var defaultCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId = managedIdentityClientId,
                ExcludeManagedIdentityCredential = environment.IsDevelopment()
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
