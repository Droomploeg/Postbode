using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.WebApp.Components.Controls.Forms;

public partial class DictionaryControl : ComponentBase
{
    [Parameter]
    public Dictionary<object, object> Items { get; set; } = [];
}
