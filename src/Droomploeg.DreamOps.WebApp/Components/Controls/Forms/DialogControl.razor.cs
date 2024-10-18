using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.WebApp.Components.Controls.Forms;

public partial class DialogControl : ComponentBase
{
    [Parameter]
    public bool IsDialogVisible { get; set; } = false;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

}
