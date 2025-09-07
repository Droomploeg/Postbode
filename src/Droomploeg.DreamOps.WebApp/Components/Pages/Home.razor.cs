using Droomploeg.DreamOps.Infrastructure.AzureServiceBus;
using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.WebApp.Components.Pages;

public partial class Home
{
    [CascadingParameter]
    public IServiceBusInfoContext ServiceBusContext { get; set; } = null!;

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            if (ServiceBusClientManager.ServiceBusInformationList.Length == 1)
            {
                ServiceBusContext.Current = ServiceBusClientManager.ServiceBusInformationList[0];
                NavigationManager.NavigateTo(PageConstants.OverviewPage);
            }

            StateHasChanged();
        }
        base.OnAfterRender(firstRender);
    }

    private bool IsSelected(ServiceBusInfo client)
    {
        return ServiceBusContext.Current == client;
    }

    private void SetClient(ServiceBusInfo selected)
    {
        ServiceBusContext.Current = selected;
        NavigationManager.NavigateTo(PageConstants.OverviewPage);
    }

}
