using System.Diagnostics.CodeAnalysis;
using Droomploeg.Postbode.Application.ServiceBus.Services;
using Droomploeg.Postbode.Domain.ServiceBus.Types;

namespace Droomploeg.Postbode.Infrastructure.AzureServiceBus.Services;

/// <summary>
/// Service bus connection manager
/// </summary>
/// <param name="connections">The available Service Bus connections.</param>
[ExcludeFromCodeCoverage( Justification = "This class is responsible for managing Service Bus connections, which is a critical part of the application's infrastructure. Testing this class would require extensive setup and may not provide significant value in terms of code coverage.")]
public class ConnectionService(IEnumerable<ServiceBusConnection> connections) : IConnectionService
{
    /// <inheritdoc cref="IConnectionService.Connections"/>
    public ServiceBusConnection[] Connections { get; } = connections.ToArray();
}
