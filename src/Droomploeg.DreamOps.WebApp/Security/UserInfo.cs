using System.Security.Claims;

namespace Droomploeg.DreamOps.WebApp.Security;

public sealed class UserInfo
{

    public required string UserId { get; init; }
    public required string Name { get; init; }

    public string[] Roles { get; init; } = Array.Empty<string>();

    public const string UserIdClaimType = "sub";
    public const string NameClaimType = "name";


    public static UserInfo FromClaimsPrincipal(ClaimsPrincipal principal) =>
        new()
        {
            UserId = GetRequiredClaim(principal, UserIdClaimType),
            Name = GetRequiredClaim(principal, NameClaimType),
            Roles = GetRequiredClaim(principal, "roles").Split(',')
        };

    public ClaimsPrincipal ToClaimsPrincipal() =>
        new(new ClaimsIdentity(
            [new(UserIdClaimType, UserId), new(NameClaimType, Name)],
            authenticationType: nameof(UserInfo),
            nameType: NameClaimType,
            roleType: null));

    private static string GetRequiredClaim(ClaimsPrincipal principal, string claimType) =>
        principal.FindFirst(claimType)?.Value ?? throw new InvalidOperationException($"Could not find required '{claimType}' claim.");
}
