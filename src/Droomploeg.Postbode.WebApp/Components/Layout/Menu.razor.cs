using Droomploeg.Postbode.Domain.ServiceBus.Types;

namespace Droomploeg.Postbode.WebApp.Components.Layout;

public partial class Menu
{
    private string _path = string.Empty;
    private bool _connectionSelected = false;

    internal async Task UpdateAsync(string path)
    {
        _path = path;

        var result = await Storage.GetAsync<ServiceBusConnection?>(nameof(ServiceBusConnection));
        if (!result.Success || result.Value is null || result.Value == ServiceBusConnection.Undefined)
        {
            _connectionSelected = false;
            StateHasChanged();
            return;
        }

        _connectionSelected = !"/".Equals(_path);
        StateHasChanged();
    }
}
