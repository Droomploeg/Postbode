using Droomploeg.DreamOps.Infrastructure.Audit;

namespace Droomploeg.DreamOps.WebApp.Configurations;

public static class AuditLoggingExtensions
{
    public static IServiceCollection AddAuditLogging(this IServiceCollection services)
    {
        return services
            .AddSingleton<IAuditContextAccessor, AuditContextAccessor>()
            .AddSingleton<IAuditLogger, AuditLogger>();
    }

    public static IApplicationBuilder UseAuditEnrichment(this IApplicationBuilder app)
        => app.UseMiddleware<AuditEnrichmentMiddleware>();
}
