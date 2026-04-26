using Droomploeg.DreamOps.WebApp.Components.Controls.Workers.Models;
using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.WebApp.Components.Controls.Workers;

public partial class NotificationContainerControl : ComponentBase
{
    [Parameter] public ICollection<NotificationModel> Items { get; set; } = [];
}

