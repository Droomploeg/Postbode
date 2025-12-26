using Droomploeg.DreamOps.Domain.ServiceBus.Types;

namespace Droomploeg.DreamOps.WebApp.Components.Layout;

public partial class Menu
{
    private string _path = string.Empty;
    private bool _connectionSelected = false;

    internal async Task UpdateAsync(string path)
    {
        _path = path;

        var result = await _storage.GetAsync<ServiceBusConnection?>(nameof(ServiceBusConnection));
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
