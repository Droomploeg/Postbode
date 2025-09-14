using System.Security.Claims;

namespace Droomploeg.DreamOps.Infrastructure.Audit;

public static class ClaimPrincipalExtensions
{
    public static string? GetUserName(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var name = principal.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(name))
        {
            return name;
        }
        var emailClaim = principal.Claims.FirstOrDefault(c =>
            string.Equals(c.Type, ClaimTypes.Email, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.Type, "email", StringComparison.OrdinalIgnoreCase));
        if (emailClaim != null && !string.IsNullOrWhiteSpace(emailClaim.Value))
        {
            return emailClaim.Value;
        }
        var upnClaim = principal.Claims.FirstOrDefault(c =>
            string.Equals(c.Type, "upn", StringComparison.OrdinalIgnoreCase));
        if (upnClaim != null && !string.IsNullOrWhiteSpace(upnClaim.Value))
        {
            return upnClaim.Value;
        }
        var nameClaim = principal.Claims.FirstOrDefault(c =>
            string.Equals(c.Type, ClaimTypes.Name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.Type, "name", StringComparison.OrdinalIgnoreCase));
        if (nameClaim != null && !string.IsNullOrWhiteSpace(nameClaim.Value))
        {
            return nameClaim.Value;
        }
        return null;
    }

    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var userIdClaim = principal.Claims.FirstOrDefault(c =>
            string.Equals(c.Type, ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.Type, "oid", StringComparison.OrdinalIgnoreCase) || 
            string.Equals(c.Type, "sub", StringComparison.OrdinalIgnoreCase)); 
        if (userIdClaim != null && !string.IsNullOrWhiteSpace(userIdClaim.Value))
        {
            return userIdClaim.Value;
        }
        return null;
    }

    public static string? GetUserTenantId(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var tenantIdClaim = principal.Claims.FirstOrDefault(c =>
            string.Equals(c.Type, "tid", StringComparison.OrdinalIgnoreCase));
        if (tenantIdClaim != null && !string.IsNullOrWhiteSpace(tenantIdClaim.Value))
        {
            return tenantIdClaim.Value;
        }
        return null;
    }
}
