using Microsoft.AspNetCore.Components;

namespace Droomploeg.Postbode.WebApp.Components.Controls.Forms;

public partial class DictionaryControl : ComponentBase
{
    [Parameter]
    public Dictionary<object, object> Items { get; set; } = [];
}
