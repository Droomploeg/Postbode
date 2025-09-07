
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus;
using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.WebApp.Components.Layout;

public partial class Menu
{
    [CascadingParameter]
    public IServiceBusInfoContext ServiceBusContext { get; set; } = null!;

    private bool HasClientSelected => ServiceBusContext.Current != ServiceBusInfo.None;
}
