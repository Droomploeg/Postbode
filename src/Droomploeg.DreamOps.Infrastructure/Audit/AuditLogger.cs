using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Droomploeg.DreamOps.Infrastructure.Audit;

public sealed class AuditLogger : IAuditLogger
{
    private const string CategoryName = "Audit";

    private readonly ILogger _logger;
    private readonly IAuditContextAccessor _context;

    public AuditLogger(ILoggerFactory factory, IAuditContextAccessor context)
    {
        _logger = factory.CreateLogger(CategoryName);  
        _context = context;
    }

    private static readonly EventId DataChangeId = new(7000, "Audit.DataChange");
    private static readonly EventId DomainActionId = new(7001, "Audit.DomainAction");

    public void DataChange(string action, string serviceBus, string entity, string? entityId, object? changes = null)
    {
        var json = changes is null ? null : JsonSerializer.Serialize(changes);
        var current = _context.Current;

        _logger.LogInformation(DataChangeId,
            "Audit DataChange {Action} {ServiceBus}.{Entity} {EntityId} {ChangesJson} " +
            "(CorrelationId={CorrelationId}, UserId={UserId}, UserName={UserName})",
            action, serviceBus, entity, entityId, json,
            current.CorrelationId, current.UserId, current.UserName);
    }

    public void DomainAction(string action, string serviceBus, string? entity = null, string? entityId = null, object? data = null)
    {
        var json = data is null ? null : JsonSerializer.Serialize(data);
        var current = _context.Current;

        _logger.LogInformation(DomainActionId,
            "Audit DomainAction {Action} {ServiceBus} {Entity} {EntityId} {DataJson} " +
            "(CorrelationId={CorrelationId}, UserId={UserId}, UserName={UserName})",
            action, serviceBus, entity, entityId, json,
            current.CorrelationId, current.UserId, current.UserName);
    }
}
