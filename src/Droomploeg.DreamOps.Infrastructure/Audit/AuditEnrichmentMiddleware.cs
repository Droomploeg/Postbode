using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;

namespace Droomploeg.DreamOps.Infrastructure.Audit;

public sealed class AuditEnrichmentMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditEnrichmentMiddleware> _logger;
    private readonly IAuditService _auditService;

    public const string CorrelationHeader = "X-Correlation-ID";

    public AuditEnrichmentMiddleware(RequestDelegate next, ILogger<AuditEnrichmentMiddleware> logger, IAuditService auditService)
    {
        _next = next;
        _logger = logger;
        _auditService = auditService;
    }

    public async Task Invoke(HttpContext httpContext, IAuditContextAccessor auditAccessor)
    {
        // var correlationId = GetCorrelationId(httpContext);
        //
        // var user = httpContext.User;
        //
        // var auditContext = new AuditContext
        // {
        //     CorrelationId = correlationId,
        //     UserId = user.GetUserId(),
        //     UserName = user.GetUserName(),
        //     Path = httpContext.Request.Path.Value,
        //     RemoteIp = httpContext.Connection.RemoteIpAddress?.ToString(),
        //     UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
        //     TenantId = user.GetTenantId()
        // };
        //
        // await auditAccessor.SetCurrentAsync(auditContext);
        //
        // using (_logger.BeginScope(new Dictionary<string, object?>
        // {
        //     ["CorrelationId"] = auditContext.CorrelationId,
        //     ["UserId"] = auditContext.UserId,
        //     ["UserName"] = auditContext.UserName,
        //     ["Path"] = auditContext.Path,
        //     ["RemoteIp"] = auditContext.RemoteIp,
        //     ["UserAgent"] = auditContext.UserAgent,
        //     ["TenantId"] = auditContext.TenantId
        // }))
        // {
        //     var start = DateTimeOffset.UtcNow;
        //     await _auditService.LogEventAsync("Audit.RequestStarted", auditContext);
        //
        //     await _next(httpContext);
        //
        //     var duration = DateTimeOffset.UtcNow - start;
        //     var completedProperties = new Dictionary<string, string?>
        //     {
        //         ["StatusCode"] = httpContext.Response?.StatusCode.ToString(),
        //         ["DurationMs"] = ((long)duration.TotalMilliseconds).ToString()
        //     };
        //
        //     await _auditService.LogEventAsync("Audit.RequestCompleted", auditContext, completedProperties);
        // }
    }

    private static string GetCorrelationId(HttpContext httpContext)
    {
        return httpContext.Request.Headers
            .TryGetValue(CorrelationHeader, out var correlactionHeader) &&
            !string.IsNullOrWhiteSpace(correlactionHeader)
                ? correlactionHeader.ToString()
                : Guid.NewGuid().ToString("n");
    }
}
