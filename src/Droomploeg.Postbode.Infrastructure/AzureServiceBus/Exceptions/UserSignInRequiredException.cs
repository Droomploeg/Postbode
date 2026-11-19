namespace Droomploeg.Postbode.Infrastructure.AzureServiceBus.Exceptions;

/// <summary>
/// Thrown when a background job can no longer get a token for the user who started it, because
/// the sign-in has expired or was revoked. The job fails instead of falling back to more rights.
/// </summary>
public sealed class UserSignInRequiredException : Exception
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="innerException">The exception of the token acquisition.</param>
    public UserSignInRequiredException(Exception innerException)
        : base("The sign-in of the user who started this job has expired or was revoked. Sign in again and restart the job.", innerException)
    {
    }
}
