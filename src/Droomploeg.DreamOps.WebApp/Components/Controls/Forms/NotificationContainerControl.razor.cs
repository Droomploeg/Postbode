using Droomploeg.DreamOps.WebApp.Components.Controls.Forms.Models;
using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.WebApp.Components.Controls.Forms;

public partial class NotificationContainerControl : ComponentBase
{
    [Parameter] public Dictionary<DateTimeOffset, IEnumerable<NotificationModel>> Items { get; set; } = [];

    private IEnumerable<NotificationModel> ItemsToDisplay()
    {
        return Items
            .OrderByDescending(i => i.Key)
            .SelectMany(i => i.Value);
    }
}

