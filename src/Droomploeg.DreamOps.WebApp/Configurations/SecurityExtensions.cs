using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Identity.Web;

namespace Droomploeg.DreamOps.WebApp.Configurations;

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
