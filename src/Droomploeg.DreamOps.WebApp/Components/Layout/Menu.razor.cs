using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;

namespace Droomploeg.DreamOps.WebApp.Components.Layout;

public partial class Menu
{
    private string _path = string.Empty;
    private bool _connectionSelected = false;
    private bool _backgroundServiceEnabled = false;

    internal async Task UpdateAsync(string path)
    {
        _path = path;

        var result = await _storage.GetAsync<ServiceBusConnectionInfo>(nameof(ServiceBusConnectionInfo));
        if (!result.Success || result.Value is null || result.Value.Connection == ServiceBusConnection.Undefined)
        {
            _connectionSelected = false;
            _backgroundServiceEnabled = false;
            StateHasChanged();
            return;
        }

        _connectionSelected = !"/".Equals(_path);
        _backgroundServiceEnabled = result.Value!.HasServiceAccount;
        StateHasChanged();
    }
}
