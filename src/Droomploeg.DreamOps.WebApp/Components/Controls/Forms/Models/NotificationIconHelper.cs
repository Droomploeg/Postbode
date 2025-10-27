using Droomploeg.DreamOps.Domain.Workers.Types;

namespace Droomploeg.DreamOps.WebApp.Components.Controls.Forms.Models;

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
