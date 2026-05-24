using Microsoft.AspNetCore.Components;

namespace Droomploeg.Postbode.WebApp.Components.Controls.AzureServiceBus;

public partial class ResubmitAllMessageControl : ComponentBase
{
    private bool _generateMessageId = false;
    private bool _deleteMessages = false;

    [Parameter] public EventCallback<(bool GenerateMessageIds, bool DeleteMessages)> OnSend { get; set; }

    [Parameter] public EventCallback OnCancel { get; set; }

    private async Task SendAsync()
    {
        if (OnSend.HasDelegate)
        {
            var args = (GenerateMessageIds: _generateMessageId, DeleteMessages: _deleteMessages);
            await OnSend.InvokeAsync(args);
        }

        _generateMessageId = false;
        _deleteMessages = false;
        StateHasChanged();
    }

    private async Task CancelAsync()
    {
        if (OnCancel.HasDelegate)
        {
            await OnCancel.InvokeAsync();
        }
        _generateMessageId = false;
        _deleteMessages = false;
    }
}
