using Droomploeg.DreamOps.Infrastructure.Audit.Disabled;
using Droomploeg.DreamOps.WebApp.Common;

namespace Droomploeg.DreamOps.WebApp.Configurations;

internal static class AuditLoggingExtensions
{
    internal static IServiceCollection AddAuditLogging(this IServiceCollection services)
    {
        return services
            .AddScoped<IAuditLogger, AuditLogger>()
            .AddScoped<IAuditContextAccessor, AuditContextAccessor>();
    }

    internal static IApplicationBuilder UseAuditEnrichment(this IApplicationBuilder app)
        => app.UseMiddleware<AuditEnrichmentMiddleware>();
}
