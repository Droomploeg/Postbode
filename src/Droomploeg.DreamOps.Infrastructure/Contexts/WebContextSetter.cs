using System.Diagnostics.CodeAnalysis;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Exceptions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Droomploeg.DreamOps.Infrastructure.Contexts;

/// <summary>
/// Web context setter to set the current application context based on the protected session storage.
/// </summary>
[ExcludeFromCodeCoverage( Justification = "This class is responsible for setting the application context based on the protected session storage, which is a critical part of the application's infrastructure. Testing this class would require extensive setup and may not provide significant value in terms of code coverage.")]
public class WebContextSetter : IContextSetter
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ProtectedSessionStorage _protectedSessionStorage;
    private readonly ApplicationContext _context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="authenticationStateProvider"><see cref="AuthenticationStateProvider"/></param>
    /// <param name="protectedSessionStorage"><see cref="ProtectedSessionStorage"/></param>
    /// <param name="context"><see cref="ApplicationContext"/></param>
    /// <exception cref="ArgumentNullException">Occurs when protectedSessionStorage or context is null</exception>
    public WebContextSetter(AuthenticationStateProvider authenticationStateProvider, ProtectedSessionStorage protectedSessionStorage, ApplicationContext context)
    {
        _authenticationStateProvider = authenticationStateProvider ?? throw new ArgumentNullException(nameof(authenticationStateProvider));
        _protectedSessionStorage = protectedSessionStorage ?? throw new ArgumentNullException(nameof(protectedSessionStorage));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc cref="IContextSetter.GetAndUpdateAsync"/>
    /// <exception cref="InvalidServiceBusConnectionException"></exception>
    public async Task<ApplicationContext> GetAndUpdateAsync()
    {
        var result = await _protectedSessionStorage.GetAsync<ServiceBusConnection?>(nameof(ServiceBusConnection));
        if (!result.Success || result.Value is null || result.Value.Value.IsNotDefined)
        {
            throw new InvalidServiceBusConnectionException();
        }

        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        _context.CurrentConnection = result.Value.Value;
        _context.UserName = user.Identity?.Name ?? "Anonymous";

        return _context;
    }
}
