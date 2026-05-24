using Droomploeg.Postbode.Domain.Workers.Types;

namespace Droomploeg.Postbode.WebApp.Components.Controls.Workers.Models;

public static class NotificationIconHelper
{
    public static string GetIcon(NotificationType type) => type switch
    {
        NotificationType.Information => "icon-information",
        NotificationType.Failure => "icon-failure",
        NotificationType.Warning => "icon-warning",
        NotificationType.Completed => "icon-success",
        _ => "icon-information"
    };
}
