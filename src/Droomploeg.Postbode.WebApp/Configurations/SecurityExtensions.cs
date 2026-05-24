using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Identity.Web;

namespace Droomploeg.Postbode.WebApp.Configurations;

[ExcludeFromCodeCoverage( Justification = "Security configuration extensions")]
internal static class SecurityExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddSecurityServices(IConfiguration configuration)
        {
            services
                .AddMicrosoftIdentityWebAppAuthentication(configuration, "AzureEntra")
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddInMemoryTokenCaches();
            services
                .AddCascadingAuthenticationState();
            services
                .AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

            return services;
        }
    }
}
