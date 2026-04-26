using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Droomploeg.DreamOps.Infrastructure.Contexts;

namespace Droomploeg.DreamOps.Aspire.Tests.Infrastructure;

/// <summary>
/// Test implementation of IContextSetter that provides a fixed application context
/// without requiring Blazor session storage or authentication state.
/// </summary>
public class TestContextSetter : IContextSetter
{
    private readonly ApplicationContext _context;

    public TestContextSetter(ApplicationContext context)
    {
        _context = context;
    }

    public Task<ApplicationContext> GetAndUpdateAsync()
    {
        return Task.FromResult(_context);
    }

    /// <summary>
    /// Creates a pre-configured ApplicationContext for testing.
    /// </summary>
    public static ApplicationContext CreateTestContext(string connectionName)
    {
        return new ApplicationContext
        {
            CorrelationId = Guid.NewGuid(),
            UserName = "test-user",
            CurrentConnection = new ServiceBusConnection(connectionName)
        };
    }
}
