using Bunit;
using Droomploeg.Postbode.Application.ServiceBus.Adapters;
using Droomploeg.Postbode.Application.ServiceBus.Services;
using Droomploeg.Postbode.Application.Workers.Dispatcher;
using Droomploeg.Postbode.Application.Workers.Services;
using Droomploeg.Postbode.Infrastructure.Audit;

namespace Droomploeg.Postbode.Aspire.Tests.Infrastructure;

/// <summary>
/// Base class for bUnit page tests that need the Service Bus emulator.
/// Initializes the shared PostbodeFixture singleton and registers application services.
/// Test classes inheriting from this can run in parallel.
/// </summary>
public abstract class PostbodeTestBase : BunitContext, IAsyncLifetime
{
    protected PostbodeFixture Fixture { get; private set; } = null!;
    protected TestAuditLogger AuditLogger { get; } = new();
    protected TestWorkerDispatcher WorkerDispatcher { get; } = new();
    protected TestRuntimeInfoService RuntimeInfoService { get; } = new();
    private TestSessionInfoProvider SessionInfoProvider { get; } = new();

    public virtual async Task InitializeAsync()
    {
        Fixture = await PostbodeFixture.GetInstanceAsync();

        Services.AddTestApplicationServices(
            Fixture.ConnectionString,
            PostbodeFixture.EmulatorConnectionName);

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
