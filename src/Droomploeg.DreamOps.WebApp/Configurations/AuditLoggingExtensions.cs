using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Droomploeg.DreamOps.Infrastructure.Audit;

namespace Droomploeg.DreamOps.WebApp.Configurations;

public static class AuditLoggingExtensions
{
    public static IServiceCollection AddAuditServices(this IServiceCollection services)
    {
        // services.AddScoped<IAuditContextAccessor, ScopedAuditContextAccessor>();
        // services.AddSingleton<IAuditService, ApplicationInsightsAuditService>();
        //
        // // TelemetryClient is registered by AddApplicationInsightsTelemetry in Program.cs
        // // but ensure a TelemetryClient is available for constructor injection
        // services.AddSingleton<TelemetryClient>(sp => {
        //     var client = sp.GetService<TelemetryClient>();
        //     return client ?? new TelemetryClient();
        // });
        //
        return services;
    }

    public static IApplicationBuilder UseAuditEnrichment(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AuditEnrichmentMiddleware>();
    }
}

