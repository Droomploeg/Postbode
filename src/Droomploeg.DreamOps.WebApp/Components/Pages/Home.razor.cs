using Droomploeg.DreamOps.Domain.ServiceBus.Types;

namespace Droomploeg.DreamOps.WebApp.Components.Pages;

public partial class Home
{
    private ServiceBusConnection _currentConnection = ServiceBusConnection.Undefined;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (ConnectionService.Connections.Length == 1)
            {
                _currentConnection = ConnectionService.Connections[0];

                await SetConnection(_currentConnection);
                return;
            }

            StateHasChanged();
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private bool IsSelected(ServiceBusConnection connection)
    {
        return _currentConnection == connection;
    }

    private async Task SetClient(ServiceBusConnection? connection)
    {
        _currentConnection = connection ?? ServiceBusConnection.Undefined;

        await SetConnection(_currentConnection);
    }

    private async Task SetConnection(ServiceBusConnection connection)
    {
        if (_currentConnection.IsNotDefined)
        {
            await Storage.DeleteAsync(nameof(ServiceBusConnection));
        }

        await Storage.SetAsync(nameof(ServiceBusConnection), connection);
        NavigationManager.NavigateTo(PageConstants.OverviewPage);
    }
}
