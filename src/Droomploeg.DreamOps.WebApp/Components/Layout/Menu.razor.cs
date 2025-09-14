using Droomploeg.DreamOps.Infrastructure.AzureServiceBus;

namespace Droomploeg.DreamOps.WebApp.Components.Layout;

public partial class Menu
{
    private ServiceBusConnection _connection = ServiceBusConnection.None;

    internal void ServiceBusSelected(ServiceBusConnection connection)
    {
        _connection = connection;
        StateHasChanged();
    }

    private bool HasServiceBusSelected => _connection != ServiceBusConnection.None;

    private bool HasBackgroundServiceEnabled => _connection.BackgroundServiceEnabled;

}
