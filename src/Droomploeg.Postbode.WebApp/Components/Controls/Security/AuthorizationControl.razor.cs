using Microsoft.AspNetCore.Components;

namespace Droomploeg.Postbode.WebApp.Components.Controls.Security;

public partial class AuthorizationControl : ComponentBase
{
    [Parameter]
    public AuthorizationState State { get; set; } = AuthorizationState.Loading;

    [Parameter]
    public AuthorizationRole[] Roles { get; set; } = [];

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
