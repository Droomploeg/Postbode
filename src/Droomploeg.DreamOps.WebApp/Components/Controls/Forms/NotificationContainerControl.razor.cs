using Droomploeg.DreamOps.WebApp.Components.Controls.Forms.Models;
using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.WebApp.Components.Controls.Forms;

public partial class NotificationContainerControl : ComponentBase
{
    [Parameter] public ICollection<NotificationModel> Items { get; set; } = [];

    private ICollection<NotificationModel> ItemsToDisplay()
    {
        return [.. Items.OrderByDescending(i => i.Timestamp)];
    }
}

