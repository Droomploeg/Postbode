using Droomploeg.DreamOps.Application.ServiceBus.Adapters;

namespace Droomploeg.DreamOps.Aspire.Tests.Infrastructure;

public class TestSessionInfoProvider : ISessionInfoProvider
{
    private readonly HashSet<string> _sessionEntities = [];

    public void RegisterSessionEntity(string name) =>
        _sessionEntities.Add(name);

    public Task<bool> RequiresSessionAsync(string queue, CancellationToken cancellationToken = default) =>
        Task.FromResult(_sessionEntities.Contains(queue));

    public Task<bool> RequiresSessionAsync(string topic, string subscription, CancellationToken cancellationToken = default) =>
        Task.FromResult(_sessionEntities.Contains(subscription));
}
