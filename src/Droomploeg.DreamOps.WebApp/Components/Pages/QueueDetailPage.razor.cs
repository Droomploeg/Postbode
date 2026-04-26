using Azure.Messaging.ServiceBus;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Droomploeg.DreamOps.WebApp.Components.Controls.AzureServiceBus.Models;
using Droomploeg.DreamOps.WebApp.Components.Controls.Security;
using Microsoft.AspNetCore.Components;
using Microsoft.Identity.Web;

namespace Droomploeg.DreamOps.WebApp.Components.Pages;

public partial class QueueDetailPage
{
    private const string DeleteAllMessagesDialog = "DeleteAllMessages";
    private const string DeleteSingleMessageDialog = "DeleteSingleMessage";
    private const string DeadLetterMessageOverlay = "DeadLetterMessage";
    private const string SendMessageOverlay = "SendMessage";
    private const string ResubmitAllMessagesOverlay = "ResubmitAllMessages";
    private const string ResubmitSingleMessageOverlay = "ResubmitSingleMessage";

    [Parameter] public string QueueName { get; set; } = null!;

    private Queue? _queue = null!;
    private ICollection<ServiceBusReceivedMessage>? _receivedMessages;
    private MessageSource _source = MessageSource.ActiveMessage;
    private ServiceBusReceivedMessage? _selectedMessage;

    private readonly Dictionary<string, bool> _visibleOverlaysAndDialogs = new()
    {
        { DeadLetterMessageOverlay, false },
        { DeleteAllMessagesDialog, false },
        { DeleteSingleMessageDialog, false },
        { SendMessageOverlay, false },
        { ResubmitAllMessagesOverlay, false },
        { ResubmitSingleMessageOverlay, false }
    };

    private AuthorizationState _authorizationState = AuthorizationState.Loading;
    private PeekModel? _peekModel;

    private bool SessionEnabled => _queue?.RequiresSession ?? false;
    private long ActiveMessageCount => _queue?.RuntimeInfo.ActiveMessageCount ?? 0;
    private long DeadLetterMessageCount => _queue?.RuntimeInfo.DeadLetterMessageCount ?? 0;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _receivedMessages = [];
            _selectedMessage = null;
            StateHasChanged();

            await Refresh();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task Refresh()
    {
        try
        {
            _queue = await RuntimeInfoService.GetQueueByNameAsync(QueueName);
            _receivedMessages = [];
            _selectedMessage = null;
            _authorizationState = AuthorizationState.Authorized;
        }
        catch (UnauthorizedAccessException)
        {
            _authorizationState = AuthorizationState.Unauthorized;
        }
        catch (MicrosoftIdentityWebChallengeUserException)
        {
            _authorizationState = AuthorizationState.TokenExpired;
        }

        StateHasChanged();
    }
    private void SourceChanged(MessageSource source)
    {
        _source = source;
        _receivedMessages = [];
        _selectedMessage = null;
        StateHasChanged();
    }

    private async Task PeekAsync(PeekModel args)
    {
        _peekModel = args;

        _receivedMessages = null;
        if (_source == MessageSource.ActiveMessage)
        {
            _receivedMessages = await UserEntityService.PeekActiveMessagesAsync(_queue!.Name, args.StartIndex, args.NumberOfMessages);
        }
        else if (_source == MessageSource.DeadLetterMessage)
        {
            _receivedMessages = await UserEntityService.PeekDeadLetterMessagesAsync(_queue!.Name, args.StartIndex, args.NumberOfMessages);
        }

        _queue = await RuntimeInfoService.GetQueueByNameAsync(_queue!.Name);
        StateHasChanged();
    }

    private void SelectMessage(ServiceBusReceivedMessage? message)
    {
        _selectedMessage = message;
        StateHasChanged();
    }

    private async Task DeadLetterSelectedMessageAsync(string reason, string description)
    {
        if (_selectedMessage == null)
        {
            return;
        }

        var result = await UserEntityService.DeadLetterMessageAsync(_queue!.Name, _selectedMessage, ApplicationConstants.ApplicationName, reason, description);
        _selectedMessage = null;
        CloseOverlaysAndDialogs();

        if (result && _peekModel != null)
        {
            await PeekAsync(_peekModel);
        }
    }

    private async Task DeleteSelectedMessageAsync()
    {
        if (_selectedMessage == null)
        {
            return;
        }

        var result = false;
        if (_source == MessageSource.ActiveMessage)
        {
            result = await UserEntityService.DeleteActiveMessageAsync(_queue!.Name, _selectedMessage, default);
        }
        if (_source == MessageSource.DeadLetterMessage)
        {
            result = await UserEntityService.DeleteDeadLetterMessageAsync(_queue!.Name, _selectedMessage, default);
        }

        _selectedMessage = null;
        CloseOverlaysAndDialogs();

        if (result && _peekModel != null)
        {
            await PeekAsync(_peekModel);
        }
    }

    private async Task ResubmitSelectedMessageAsync(SendMessageModel model)
    {
        if (_selectedMessage == null)
        {
            return;
        }

        var result = await UserEntityService.ResubmitMessageAsync(
            QueueName,
            _selectedMessage,
            model.ToSendMessage(),
            new ResubmitOptions(model.GenerateMessageId, model.DeleteMessageAfterResubmit));

        CloseOverlaysAndDialogs();

        if (result && _peekModel != null)
        {
            await PeekAsync(_peekModel);
        }
    }

    private async Task SendMessageAsync(SendMessageModel model)
    {
        var result = await UserEntityService.SendMessageAsync(QueueName, model.ToSendMessage());

        CloseOverlaysAndDialogs();

        if (result && _peekModel != null)
        {
            await PeekAsync(_peekModel);
        }
    }

    private async Task ResubmitAllMessages(bool generateMessageIds, bool deleteMesssages)
    {
        var resubmitOptions = new ResubmitOptions(generateMessageIds, deleteMesssages);

        var result = await UserEntityService.ResubmitAllMessagesAsync(QueueName, resubmitOptions);

        CloseOverlaysAndDialogs();

        if (result && _peekModel != null)
        {
            await PeekAsync(_peekModel);
        }
    }

    private async Task DeleteAllMessages()
    {
        var result = false;
        if (_source == MessageSource.ActiveMessage)
        {
            result = await UserEntityService.DeleteAllActiveMessagesAsync(QueueName);
        }
        else if (_source == MessageSource.DeadLetterMessage)
        {
            result = await UserEntityService.DeleteAllDeadLetterMessagesAsync(QueueName);
        }

        CloseOverlaysAndDialogs();

        if (result && _peekModel != null)
        {
            await PeekAsync(_peekModel);
        }
    }

    private void ShowOverlayOrDialog(string overlayOrDialog, ServiceBusReceivedMessage? message = null)
    {
        CloseOverlaysAndDialogs();

        _selectedMessage = message;
        _visibleOverlaysAndDialogs[overlayOrDialog] = true;
        StateHasChanged();
    }

    private void CloseOverlaysAndDialogs()
    {
        foreach (var key in _visibleOverlaysAndDialogs.Keys)
        {
            _visibleOverlaysAndDialogs[key] = false;
        }

        StateHasChanged();
    }
}
