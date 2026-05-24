using System.Diagnostics.CodeAnalysis;
using Droomploeg.Postbode.Infrastructure.Contexts;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;

namespace Droomploeg.Postbode.Infrastructure.Audit;

/// <summary>
/// Audit the logger implementation that logs user actions with correlation tracking.
/// Prefers TelemetryClient for structured events, falls back to ILogger if unavailable.
/// </summary>
[ExcludeFromCodeCoverage( Justification = "Logger class")]
public class AuditLogger : IAuditLogger
{
    private readonly ILogger<AuditLogger> _logger;
    private readonly IContextSetter _contextSetter;
    private readonly TelemetryClient? _telemetryClient;

    /// <summary>
    /// Initializes a new instance of <see cref="AuditLogger"/>.
    /// </summary>
    /// <param name="logger">The logger for fallback and informational output.</param>
    /// <param name="contextSetter">The context setter for retrieving user and correlation information.</param>
    /// <param name="telemetryClient">Optional Application Insights telemetry client for structured event logging.</param>
    public AuditLogger(
        ILogger<AuditLogger> logger,
        IContextSetter contextSetter,
        TelemetryClient? telemetryClient = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _contextSetter = contextSetter ?? throw new ArgumentNullException(nameof(contextSetter));
        _telemetryClient = telemetryClient;
    }

    /// <inheritdoc cref="IAuditLogger.LogAuditAsync"/>
    public async Task LogAuditAsync(string action, string resource, string? details = null)
    {
        var context = await _contextSetter.GetAndUpdateAsync();

        if (_telemetryClient != null)
        {
            // Use Application Insights custom events for better querying and analytics
            var properties = new Dictionary<string, string>
            {
                ["UserName"] = context.UserName,
                ["Action"] = action,
                ["Resource"] = resource,
                ["CorrelationId"] = context.CorrelationId.ToString(),
                ["Details"] = details ?? "N/A",
                ["Timestamp"] = DateTimeOffset.UtcNow.ToString("o")
            };

            var telemetry = new EventTelemetry("AuditEvent");

            foreach (var property in properties)
            {
                telemetry.Properties.Add(property.Key, property.Value);
            }

            _telemetryClient.TrackEvent(telemetry);
            _logger.LogInformation(
                "AUDIT: User={UserName}, Action={Action}, Resource={Resource}, CorrelationId={CorrelationId}, Details={Details}",
                context.UserName,
                action,
                resource,
                context.CorrelationId,
                details ?? "N/A");
        }
        else
        {
            // Fallback to ILogger if TelemetryClient is not available
            _logger.LogInformation(
                "AUDIT: User={UserName}, Action={Action}, Resource={Resource}, CorrelationId={CorrelationId}, Details={Details}",
                context.UserName,
                action,
                resource,
                context.CorrelationId,
                details ?? "N/A");
        }
    }
}
