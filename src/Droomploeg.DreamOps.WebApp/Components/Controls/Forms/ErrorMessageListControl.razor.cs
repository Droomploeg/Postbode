using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.WebApp.Components.Controls.Forms;

public partial class ErrorMessageListControl : ComponentBase
{
    [Parameter]
    public ICollection<ServiceBusReceivedMessage>? Messages { get; set; } = null;
}
