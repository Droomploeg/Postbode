using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Droomploeg.Postbode.WebApp.Components.Controls.Security;

public partial class LoginControl : ComponentBase, IDisposable
{
    //private string? _currentUrl;

    protected override void OnInitialized()
    {
        //_currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        //currentUrl = NavigationManager.ToBaseRelativePath(e.Location);
        //_currentUrl = "/";
        StateHasChanged();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        NavigationManager.LocationChanged -= OnLocationChanged;
    }
}
