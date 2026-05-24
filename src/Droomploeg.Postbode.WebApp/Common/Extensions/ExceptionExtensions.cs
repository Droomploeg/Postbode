using Microsoft.Identity.Client;
using Microsoft.Identity.Web;

namespace Droomploeg.Postbode.WebApp.Common.Extensions;

public static class ExceptionExtensions
{
    public static bool IsAuthorizationException(this Exception exception)
        => exception is UnauthorizedAccessException ||
           exception is MsalUiRequiredException;
}
