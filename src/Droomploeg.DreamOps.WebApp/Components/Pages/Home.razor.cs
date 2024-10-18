using Droomploeg.DreamOps.Infrastructure.AzureServiceBus;
using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.WebApp.Components.Pages;

public partial class Home
{
    [CascadingParameter]
    public IServiceBusClientContext ServiceBusContext { get; set; }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            if (ServiceBusClientManager.Clients.Length == 1)
            {
                ServiceBusContext.CurrentClient = ServiceBusClientManager.Clients[0];
                NavigationManager.NavigateTo(PageConstants.OverviewPage);
            }

            StateHasChanged();
        }
        base.OnAfterRender(firstRender);
    }

    private bool IsSelected(string client)
    {
        return ServiceBusContext.CurrentClient == client;
    }

    private void SetClient(string client)
    {
        ServiceBusContext.CurrentClient = client;
        NavigationManager.NavigateTo(PageConstants.OverviewPage);
    }

}
