using Droomploeg.DreamOps.Domain.ServiceBus.Models;

namespace Droomploeg.DreamOps.WebApp.Components.Pages;

public partial class Home
{
    private ServiceBusConnectionInfo _currentConnectionInfo = ServiceBusConnectionInfo.Undefined;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (_connectionService.Connections.Length == 1)
            {
                _currentConnectionInfo = _connectionService.Connections[0];

                await SetConnection(_currentConnectionInfo);
                return;
            }

            StateHasChanged();
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private bool IsSelected(ServiceBusConnectionInfo connectionInfo)
    {
        return (_currentConnectionInfo == connectionInfo);
    }

    private async Task SetClient(ServiceBusConnectionInfo? connectionInfo)
    {
        _currentConnectionInfo = connectionInfo ?? ServiceBusConnectionInfo.Undefined;

        await SetConnection(_currentConnectionInfo);
    }

    private async Task SetConnection(ServiceBusConnectionInfo connectionInfo)
    {
        if (_currentConnectionInfo.Connection.IsNotDefined)
        {
            await _storage.DeleteAsync(nameof(ServiceBusConnectionInfo));
        }

        await _storage.SetAsync(nameof(ServiceBusConnectionInfo), connectionInfo);
        _navigationManager.NavigateTo(PageConstants.OverviewPage);
    }
}
