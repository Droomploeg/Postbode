using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Microsoft.Identity.Web;

namespace Droomploeg.DreamOps.WebApp.Security;

[ExcludeFromCodeCoverage( Justification = "Custom token credential for on-behalf-of authentication")]
public class OnBehalfOfTokenCredential : TokenCredential
{
    public const string ServiceBusScope = "https://servicebus.azure.net/.default";

    private readonly IServiceProvider _serviceProvider;
    private readonly string[] _scopes;

    public OnBehalfOfTokenCredential(IServiceProvider serviceProvider, string[] scopes)
    {
        _serviceProvider = serviceProvider;
        _scopes = scopes;
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var tokenAcquisition = scope.ServiceProvider.GetRequiredService<ITokenAcquisition>();
        var token = tokenAcquisition.GetAccessTokenForUserAsync(_scopes).Result;
        return new AccessToken(token, DateTimeOffset.UtcNow.AddMinutes(10));
    }

    public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var tokenAcquisition = scope.ServiceProvider.GetRequiredService<ITokenAcquisition>();
        var token = await tokenAcquisition.GetAccessTokenForUserAsync(_scopes);
        return new AccessToken(token, DateTimeOffset.UtcNow.AddMinutes(10));
    }
}
