using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

namespace Droomploeg.DreamOps.Infrastructure.Audit;

public sealed class ApplicationInsightsAuditService : IAuditService
{
    private readonly TelemetryClient? _telemetryClient;
    private readonly ILogger<ApplicationInsightsAuditService> _logger;

    public ApplicationInsightsAuditService(TelemetryClient? telemetryClient, ILogger<ApplicationInsightsAuditService> logger)
    {
        _telemetryClient = telemetryClient;
        _logger = logger;
    }

    public ValueTask LogEventAsync(string name, IAuditContext? context = null, IDictionary<string, string?>? properties = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var props = new Dictionary<string, string?>
            {
                ["CorrelationId"] = context?.CorrelationId,
                ["UserId"] = context?.UserId,
                ["UserName"] = context?.UserName,
                ["Path"] = context?.Path,
                ["RemoteIp"] = context?.RemoteIp,
                ["UserAgent"] = context?.UserAgent,
                ["TenantId"] = context?.TenantId,
                ["Audit"] = "true"
            };

            if (context?.Properties != null)
            {
                foreach (var kv in context.Properties)
                {
                    if (!props.ContainsKey(kv.Key)) props[kv.Key] = kv.Value;
                }
            }

            if (properties != null)
            {
                foreach (var kv in properties)
                {
                    props[kv.Key] = kv.Value;
                }
            }

            if (_telemetryClient != null)
            {
                // TelemetryClient.TrackEvent accepts IDictionary<string, string>
                var nonNull = props.Where(kv => kv.Value != null).ToDictionary(kv => kv.Key, kv => kv.Value!);
                _telemetryClient.TrackEvent(name, nonNull);
                return ValueTask.CompletedTask;
            }

            // Fallback to ILogger
            _logger.LogInformation("AuditEvent {Name} {@Props}", name, props);
            return ValueTask.CompletedTask;
        }
        catch (Exception ex)
        {
            // Never throw from audit logging
            _logger.LogError(ex, "Failed to write audit event {Name}", name);
            return ValueTask.CompletedTask;
        }
    }
}
