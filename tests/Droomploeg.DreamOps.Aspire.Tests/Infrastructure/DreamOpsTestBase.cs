using Bunit;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Application.ServiceBus.Services;
using Droomploeg.DreamOps.Application.Workers.Dispatcher;
using Droomploeg.DreamOps.Application.Workers.Services;
using Droomploeg.DreamOps.Infrastructure.Audit;

namespace Droomploeg.DreamOps.Aspire.Tests.Infrastructure;

/// <summary>
/// Base class for bUnit page tests that need the Service Bus emulator.
/// Initializes the shared DreamOpsFixture singleton and registers application services.
/// Test classes inheriting from this can run in parallel.
/// </summary>
public abstract class DreamOpsTestBase : BunitContext, IAsyncLifetime
{
    protected DreamOpsFixture Fixture { get; private set; } = null!;
    protected TestAuditLogger AuditLogger { get; } = new();
    protected TestWorkerDispatcher WorkerDispatcher { get; } = new();
    protected TestRuntimeInfoService RuntimeInfoService { get; } = new();
    private TestSessionInfoProvider SessionInfoProvider { get; } = new();

    public virtual async Task InitializeAsync()
    {
        Fixture = await DreamOpsFixture.GetInstanceAsync();

        Services.AddTestApplicationServices(
            Fixture.ConnectionString,
            DreamOpsFixture.EmulatorConnectionName);

        AddAuthorization().SetAuthorized("test-user");

        Services.AddSingleton<IRuntimeInfoService>(RuntimeInfoService);
        Services.AddSingleton<ISessionInfoProvider>(SessionInfoProvider);

        // Override with test instances for inspection
        Services.AddSingleton(AuditLogger);
        Services.AddSingleton<IAuditLogger>(AuditLogger);
        Services.AddSingleton(WorkerDispatcher);
        Services.AddSingleton<IWorkerDispatcher>(sp =>
        {
            WorkerDispatcher.SetWorkerService(sp.GetRequiredService<IWorkerService>());
            return WorkerDispatcher;
        });
    }

    protected void RegisterSessionEntity(string name)
    {
        RuntimeInfoService.RegisterSessionEntity(name);
        SessionInfoProvider.RegisterSessionEntity(name);
    }

    public new Task DisposeAsync() => Task.CompletedTask;
}
