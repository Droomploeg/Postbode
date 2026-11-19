using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.Postbode.Application.ServiceBus.Adapters;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Factories;
using Droomploeg.Postbode.Infrastructure.Contexts;

namespace Droomploeg.Postbode.Infrastructure.AzureServiceBus.Adapters;

/// <summary>
/// Provides session information for Service Bus entities using the <see cref="ServiceBusAdministrationClient"/>.
/// Queries the Service Bus management API to determine whether a queue or subscription requires sessions.
/// Returns <see langword="false"/> when the management API is unavailable (e.g. emulator).
/// </summary>
[ExcludeFromCodeCoverage( Justification = "This class is responsible for retrieving session information from Azure Service Bus, which is a critical part of the application's infrastructure. Testing this class would require extensive setup and may not provide significant value in terms of code coverage.")]
public class SessionInfoProvider : ISessionInfoProvider
{
    private readonly ApplicationContext _context;
    private readonly IServiceAccountAdminClientProvider _adminClientProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="SessionInfoProvider"/>.
    /// </summary>
    /// <param name="context"><see cref="ApplicationContext"/> for the current request.</param>
    /// <param name="adminClientProvider">Provider of the service account <see cref="ServiceBusAdministrationClient"/>.</param>
    public SessionInfoProvider(
        ApplicationContext context,
        IServiceAccountAdminClientProvider adminClientProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _adminClientProvider = adminClientProvider ?? throw new ArgumentNullException(nameof(adminClientProvider));
    }

    /// <inheritdoc cref="ISessionInfoProvider.RequiresSessionAsync(string, CancellationToken)"/>
    public async Task<bool> RequiresSessionAsync(string queue, CancellationToken cancellationToken = default)
    {
        try
        {
            var adminClient = _adminClientProvider.GetClient(_context.CurrentConnection);
            var response = await adminClient.GetQueueAsync(queue, cancellationToken);
            return response.Value.RequiresSession;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc cref="ISessionInfoProvider.RequiresSessionAsync(string, string, CancellationToken)"/>
    public async Task<bool> RequiresSessionAsync(string topic, string subscription, CancellationToken cancellationToken = default)
    {
        try
        {
            var adminClient = _adminClientProvider.GetClient(_context.CurrentConnection);
            var response = await adminClient.GetSubscriptionAsync(topic, subscription, cancellationToken);
            return response.Value.RequiresSession;
        }
        catch
        {
            return false;
        }
    }
}
