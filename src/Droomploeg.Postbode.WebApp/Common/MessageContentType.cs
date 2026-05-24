namespace Droomploeg.Postbode.WebApp.Common;

internal static class MessageContentType
{
    internal static bool IsJson(string contentType)
    {
        return "application/json".Equals(contentType, StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsText(string contentType)
    {
        return "text/plain".Equals(contentType, StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsXml(string contentType)
    {
        return "application/xml".Equals(contentType, StringComparison.OrdinalIgnoreCase);
    }
}
