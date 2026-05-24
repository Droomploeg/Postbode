using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus.Administration;
using Droomploeg.Postbode.Application.ServiceBus.Adapters;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Extensions;
using Droomploeg.Postbode.Infrastructure.Contexts;
using Microsoft.Extensions.Azure;

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
    private readonly IAzureClientFactory<ServiceBusAdministrationClient> _adminClientFactory;

    /// <summary>
    /// Initializes a new instance of <see cref="SessionInfoProvider"/>.
    /// </summary>
    /// <param name="context"><see cref="ApplicationContext"/> for the current request.</param>
    /// <param name="adminClientFactory">Factory for creating <see cref="ServiceBusAdministrationClient"/> instances.</param>
    public SessionInfoProvider(
        ApplicationContext context,
        IAzureClientFactory<ServiceBusAdministrationClient> adminClientFactory)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _adminClientFactory = adminClientFactory ?? throw new ArgumentNullException(nameof(adminClientFactory));
    }

    /// <inheritdoc cref="ISessionInfoProvider.RequiresSessionAsync(string, CancellationToken)"/>
    public async Task<bool> RequiresSessionAsync(string queue, CancellationToken cancellationToken = default)
    {
        try
        {
            var adminClient = _adminClientFactory.CreateClient(_context);
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
            var adminClient = _adminClientFactory.CreateClient(_context);
            var response = await adminClient.GetSubscriptionAsync(topic, subscription, cancellationToken);
            return response.Value.RequiresSession;
        }
        catch
        {
            return false;
        }
    }
}
