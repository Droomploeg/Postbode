using Azure.Messaging.ServiceBus;
using Droomploeg.DreamOps.Core.Models;
using Droomploeg.DreamOps.Infrastructure.HostedServices.WorkerService;
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
    private IEnumerable<ServiceBusReceivedMessage>? _receivedMessages;
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

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await Refresh();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            _receivedMessages = [];
            _selectedMessage = null;
            StateHasChanged();
        }
        base.OnAfterRender(firstRender);
    }

    private async Task Refresh()
    {
        try
        {
            _queue = await ServiceBusService.GetQueueByNameAsync(QueueName);
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

        if (_queue == null)
        {
            return;
        }

        if (_source == MessageSource.ActiveMessage)
        {
            _receivedMessages = null;
            _receivedMessages = await ActiveEntityService.PeekAsync(_queue.Name, args.StartIndex, args.NumberOfMessages);
        }
        else if (_source == MessageSource.DeadLetterMessage)
        {
            _receivedMessages = null;
            _receivedMessages = await DeadLetterEntityService.PeekAsync(_queue.Name, args.StartIndex, args.NumberOfMessages);
        }

        _queue = await ServiceBusService.GetQueueByNameAsync(_queue.Name);
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

        // todo general: bool back
        await ActiveEntityService.DeadLetterMessageAsync(_queue!.Name, _selectedMessage, ApplicationConstants.ApplicationName, reason, description);

        _selectedMessage = null;
        CloseOverlaysAndDialogs();

        if (_peekModel != null)
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

        if (_source == MessageSource.ActiveMessage)
        {
            await ActiveEntityService.DeleteMessageAsync(_queue!.Name, _selectedMessage, default);
        }
        if (_source == MessageSource.DeadLetterMessage)
        {
            await DeadLetterEntityService.DeleteMessageAsync(_queue!.Name, _selectedMessage, default);
        }

        _selectedMessage = null;
        CloseOverlaysAndDialogs();

        if (_peekModel != null)
        {
            await PeekAsync(_peekModel);
        }
    }

    //todo background: resubmit in background (queue)
    private async Task ResubmitSelectedMessageAsync(SendMessageModel model)
    {
        if (_selectedMessage == null)
        {
            return;
        }

        async Task action(CancellationToken cancellationToken) =>
            await DeadLetterEntityService.ResubmitMessageAsync(QueueName,
                _selectedMessage,
                model.ToSendMessage(),
                new ResubmitOptions(model.GenenerateMessageId, model.DeleteMessageAfterResubmit),
                cancellationToken);

        var workItem = new WorkItem(QueueName, "Send messages", action);
        WorkerService.Register(workItem);

        CloseOverlaysAndDialogs();

        if (_peekModel != null)
        {
            await PeekAsync(_peekModel);
        }
    }

    //todo background: send message in background (queue)
    private async Task SendMessageAsync(SendMessageModel model)
    {
        async Task action(CancellationToken cancellationToken) =>
            await ActiveEntityService.SendMessageAsync(QueueName, [model.ToSendMessage()], cancellationToken);

        // todo general: const for action name
        var workItem = new WorkItem(QueueName, "Send messages", action);
        WorkerService.Register(workItem);

        CloseOverlaysAndDialogs();

        if (_peekModel != null)
        {
            await PeekAsync(_peekModel);
        }
    }

    //todo background: resubmit all message in background (queue)
    private void ResubmitAllMessages(bool generateMessageIds, bool deleteMesssages)
    {
        var resubmitOptions = new ResubmitOptions(generateMessageIds, deleteMesssages);
        async Task action(CancellationToken cancellationToken)
            => await DeadLetterEntityService.ResubmitAllMessagesAsync(QueueName, resubmitOptions, cancellationToken);

        // todo general: const for action name
        var workItem = new WorkItem(QueueName, "Resubmit all dead-letter messages", action);
        WorkerService.Register(workItem);

        CloseOverlaysAndDialogs();
    }

    //todo background: delete all message in background (queue)
    private void DeleteAllMessages()
    {
        Func<CancellationToken, Task> action = _source == MessageSource.ActiveMessage
            ? async (cancellationToken) => await ActiveEntityService.DeleteAllMessagesAsync(QueueName, cancellationToken)
            : async (cancellationToken) => await DeadLetterEntityService.DeleteAllMessagesAsync(QueueName, cancellationToken);

        // todo general: const for action name
        var workItem = new WorkItem(QueueName, "Delete all messages", action);
        WorkerService.Register(workItem);

        CloseOverlaysAndDialogs();
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
