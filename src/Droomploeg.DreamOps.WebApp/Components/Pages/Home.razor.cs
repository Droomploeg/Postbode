using Droomploeg.DreamOps.Infrastructure.AzureServiceBus;
using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.WebApp.Components.Pages;

public partial class Home
{
    [Inject]
    public IServiceBusConnectionAccessor ServiceBusContext { get; set; } = null!;
    
    private ServiceBusConnection _connection = ServiceBusConnection.None;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (ServiceBusClientManager.ServiceBusInformationList.Length == 1)
            {
                _connection = ServiceBusClientManager.ServiceBusInformationList[0];

                await ServiceBusContext.SetCurrentAsync(ServiceBusClientManager.ServiceBusInformationList[0]);
                NavigationManager.NavigateTo(PageConstants.OverviewPage);
            }

            StateHasChanged();
        }
        base.OnAfterRender(firstRender);
    }

    private bool IsSelected(ServiceBusConnection client)
    {
        return _connection == client;
    }

    private async Task SetClient(ServiceBusConnection selected)
    {
        await ServiceBusContext.SetCurrentAsync(selected);
        NavigationManager.NavigateTo(PageConstants.OverviewPage);
    }

}
