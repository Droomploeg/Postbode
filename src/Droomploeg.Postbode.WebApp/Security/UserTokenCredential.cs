using System.Security.Claims;
using Azure.Core;
using Microsoft.Identity.Web;

namespace Droomploeg.Postbode.WebApp.Security;

/// <summary>
/// Token credential that acquires a delegated Service Bus token for one specific user.
/// The user is fixed at construction, so the credential never depends on an HttpContext and
/// can never hand out another user's token.
/// </summary>
internal sealed partial class UserTokenCredential : TokenCredential
{
    internal const string ServiceBusScope = "https://servicebus.azure.net/.default";

    private static readonly string[] Scopes = [ServiceBusScope];

    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly ClaimsPrincipal _user;
    private readonly ILogger<UserTokenCredential> _logger;

    public UserTokenCredential(
        ITokenAcquisition tokenAcquisition,
        ClaimsPrincipal user,
        ILogger<UserTokenCredential> logger)
    {
        _tokenAcquisition = tokenAcquisition ?? throw new ArgumentNullException(nameof(tokenAcquisition));
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        // The Service Bus SDK only uses the asynchronous path; this overload exists to satisfy TokenCredential.
        => GetTokenAsync(requestContext, cancellationToken).AsTask().GetAwaiter().GetResult();

    public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _tokenAcquisition.GetAuthenticationResultForUserAsync(
                Scopes,
                user: _user,
                tokenAcquisitionOptions: new TokenAcquisitionOptions { CancellationToken = cancellationToken });

            LogTokenAcquired(result.ExpiresOn);

            // Pass the real expiry so the Service Bus SDK refreshes the token exactly when needed.
            return new AccessToken(result.AccessToken, result.ExpiresOn);
        }
        catch (MicrosoftIdentityWebChallengeUserException exception)
        {
            LogTokenUnavailable(exception);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Acquired delegated Service Bus token, expires on {ExpiresOn}")]
    private partial void LogTokenAcquired(DateTimeOffset expiresOn);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "No delegated Service Bus token available without user interaction")]
    private partial void LogTokenUnavailable(Exception exception);
}
