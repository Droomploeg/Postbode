using Droomploeg.DreamOps.Infrastructure.Audit;
using Droomploeg.DreamOps.WebApp.Common;

namespace Droomploeg.DreamOps.WebApp.Configurations;

public static class AuditLoggingExtensions
{
    public static IServiceCollection AddAuditLogging(this IServiceCollection services)
    {
        return services
            .AddScoped<IAuditLogger, AuditLogger>()
            .AddScoped<IAuditContextAccessor, AuditContextAccessor>();
    }

    public static IApplicationBuilder UseAuditEnrichment(this IApplicationBuilder app)
        => app.UseMiddleware<AuditEnrichmentMiddleware>();
}
