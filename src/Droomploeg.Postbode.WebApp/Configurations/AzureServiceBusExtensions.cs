using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using Droomploeg.Postbode.Application.ServiceBus.Services;
using Droomploeg.Postbode.Domain.ServiceBus.Types;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Factories;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Services;
using Droomploeg.Postbode.WebApp.Common;
using Droomploeg.Postbode.WebApp.Configurations.Options;
using Droomploeg.Postbode.WebApp.Security;

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

            IReadOnlyDictionary<string, string> namespaces = configConnectionList
                .ToDictionary(c => c.Name, c => c.FullyQualifiedNamespace, StringComparer.OrdinalIgnoreCase);

            // The service account (managed identity) only reads metadata. Its administration
            // clients are shared: they always act as the same identity.
            services.AddSingleton<IServiceAccountAdminClientProvider>(sp =>
                ActivatorUtilities.CreateInstance<ServiceAccountAdminClientProvider>(sp, namespaces, defaultCredential));

            // Message operations run with the token of the user, on a client per circuit or
            // background job, so no AMQP connection or cached token is shared between users.
            services.AddScoped<IUserServiceBusClientProvider>(sp =>
                ActivatorUtilities.CreateInstance<UserServiceBusClientProvider>(sp, namespaces));

            var connectionList = configConnectionList
                .Select(c => new ServiceBusConnection(c.Name));

            var connectionService = new ConnectionService(connectionList);
            services
                .AddSingleton<IConnectionService>(connectionService)
                .AddTransient<ApplicationInsightsLink>();

            return services;
        }
    }
}
