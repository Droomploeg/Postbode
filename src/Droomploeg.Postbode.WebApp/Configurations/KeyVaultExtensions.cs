using System.Diagnostics.CodeAnalysis;
using Azure.Identity;

namespace Droomploeg.Postbode.WebApp.Configurations;

[ExcludeFromCodeCoverage(Justification = "Key Vault configuration source")]
internal static class KeyVaultExtensions
{
    extension(WebApplicationBuilder builder)
    {
        internal WebApplicationBuilder AddKeyVaultConfiguration()
        {
            // The vault URL is provided as an app setting (see bicep/appServices/webapp.bicep).
            // When it is absent (e.g. local development without a vault) configuration falls back
            // to appsettings/user-secrets and Key Vault is simply not registered.
            var keyVaultUri = builder.Configuration["AzureKeyVault"];
            if (string.IsNullOrWhiteSpace(keyVaultUri))
            {
                return builder;
            }

            // Managed identity on Azure (AZURE_CLIENT_ID / ManagedIdentityClientId), developer
            // credentials locally. Excluding the managed-identity probe in Development avoids the
            // unreachable IMDS endpoint at 169.254.169.254.
            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId = builder.Configuration["ManagedIdentityClientId"],
                ExcludeManagedIdentityCredential = builder.Environment.IsDevelopment()
            });

            builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), credential);

            return builder;
        }
    }
}
