namespace Droomploeg.DreamOps.Core.Models;

/// <summary>
/// Current user context interface
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// User identifier
    /// </summary>
    string? UserId { get; }
    /// <summary>
    /// User name
    /// </summary>
    string? UserName { get; }
    /// <summary>
    /// Email
    /// </summary>
    string? Email { get; }
}
