using Azure.Messaging.ServiceBus;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Droomploeg.DreamOps.WebApp.Components.Controls.AzureServiceBus.Models;
using Droomploeg.DreamOps.WebApp.Components.Controls.Forms;
using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.WebApp.Components.Controls.AzureServiceBus;

public partial class PeekControl : ComponentBase
{
    [SupplyParameterFromForm] private PeekModel Model { get; set; } = new PeekModel();

    [Parameter] public bool SessionEnabled { get; set; } = false;

    [Parameter] public ICollection<ServiceBusReceivedMessage>? Messages { get; set; }

    [Parameter] public long ActiveMessageCount { get; set; }

    [Parameter] public long DeadLetterMessageCount { get; set; }

    [Parameter] public EventCallback<PeekModel> OnPeek { get; set; }

    [Parameter] public EventCallback OnResubmitAll { get; set; }

    [Parameter] public EventCallback OnDeleteAll { get; set; }

    [Parameter] public EventCallback<MessageSource> OnMessageSourceChanged { get; set; }

    [Parameter] public EventCallback<ServiceBusReceivedMessage> OnDelete { get; set; }

    [Parameter] public EventCallback<ServiceBusReceivedMessage> OnDeadLetter { get; set; }

    [Parameter] public EventCallback<ServiceBusReceivedMessage> OnResubmit { get; set; }

    [Parameter] public EventCallback<ServiceBusReceivedMessage> OnSelected { get; set; }

    private MessageSource _source = MessageSource.ActiveMessage;
    private string ActiveMessageTabTitle => $"Active ({ActiveMessageCount})";
    private string DeadLetterMessageTabTitle => $"Dead-letter ({DeadLetterMessageCount})";

    private async Task PeekAsync()
    {
        if (OnPeek.HasDelegate)
        {
            await OnPeek.InvokeAsync(Model);
        }
    }

    private async Task ResubmitAllAsync()
    {
        if (OnPeek.HasDelegate)
        {
            await OnResubmitAll.InvokeAsync();
        }
    }

    private async Task DeleteAllAsync()
    {
        if (OnPeek.HasDelegate)
        {
            await OnDeleteAll.InvokeAsync();
        }
    }

    private async Task ChangeMessageSourceAsync(TabItemControl tabItem)
    {
        var value = (MessageSource)tabItem.Tag!;
        if (value == _source)
        {
            return;
        }

        _source = value;
        if (OnMessageSourceChanged.HasDelegate)
        {
            await OnMessageSourceChanged.InvokeAsync(_source);
        }

        await SelectedAsync(null);
    }

    private async Task DeleteAsync(ServiceBusReceivedMessage message)
    {
        if (OnDelete.HasDelegate)
        {
            await OnDelete.InvokeAsync(message);
        }
    }

    private async Task DeadLetterAsync(ServiceBusReceivedMessage message)
    {
        if (OnDeadLetter.HasDelegate)
        {
            await OnDeadLetter.InvokeAsync(message);
        }
    }

    private async Task ResubmitAsync(ServiceBusReceivedMessage message)
    {
        if (OnResubmit.HasDelegate)
        {
            await OnResubmit.InvokeAsync(message);
        }
    }

    private async Task SelectedAsync(ServiceBusReceivedMessage? message)
    {
        if (OnSelected.HasDelegate)
        {
            await OnSelected.InvokeAsync(message);
        }
    }
}
