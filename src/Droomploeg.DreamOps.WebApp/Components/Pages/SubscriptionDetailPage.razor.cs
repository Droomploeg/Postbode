using Azure.Messaging.ServiceBus;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Droomploeg.DreamOps.WebApp.Components.Controls.AzureServiceBus.Models;
using Droomploeg.DreamOps.WebApp.Components.Controls.Security;
using Microsoft.AspNetCore.Components;
using Microsoft.Identity.Web;

namespace Droomploeg.DreamOps.WebApp.Components.Pages;

public partial class SubscriptionDetailPage
{
    private const string DeleteAllMessagesDialog = "DeleteAllMessages";
    private const string DeleteSingleMessageDialog = "DeleteSingleMessage";
    private const string DeadLetterMessageOverlay = "DeadLetterMessage";
    private const string SendMessageOverlay = "SendMessage";
    private const string ResubmitAllMessagesOverlay = "ResubmitAllMessages";
    private const string ResubmitSingleMessageOverlay = "ResubmitSingleMessage";

    [Parameter] public string TopicName { get; set; } = null!;
    [Parameter] public string SubscriptionName { get; set; } = null!;

    private AuthorizationState _authorizationState = AuthorizationState.Loading;
    private Subscription? _subscription;

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

    private PeekModel? _peekModel;

    private bool SessionEnabled => _subscription?.RequiresSession ?? false;
    private long ActiveMessageCount => _subscription?.RuntimeInfo.ActiveMessageCount ?? 0;
    private long DeadLetterMessageCount => _subscription?.RuntimeInfo.DeadLetterMessageCount ?? 0;

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
            _subscription = await _runtimeInfoService.GetSubscriptionAsync(TopicName, SubscriptionName);
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
            _receivedMessages = await _userTopicService.PeekActiveMessagesAsync(TopicName, SubscriptionName, args.StartIndex, args.NumberOfMessages);
        }
        else if (_source == MessageSource.DeadLetterMessage)
        {
            _receivedMessages = await _userTopicService.PeekDeadLetterMessagesAsync(TopicName, SubscriptionName, args.StartIndex, args.NumberOfMessages);
        }

        _subscription = await _runtimeInfoService.GetSubscriptionAsync(TopicName, SubscriptionName);
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

        await _userTopicService.DeadLetterMessageAsync(TopicName, SubscriptionName, _selectedMessage, ApplicationConstants.ApplicationName, reason, description);

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
            await _userTopicService.DeleteActiveMessageAsync(TopicName, SubscriptionName, _selectedMessage, default);
        }
        if (_source == MessageSource.DeadLetterMessage)
        {
            await _userTopicService.DeleteDeadLetterMessageAsync(TopicName, SubscriptionName, _selectedMessage, default);
        }

        _selectedMessage = null;
        CloseOverlaysAndDialogs();

        if (_peekModel != null)
        {
            await PeekAsync(_peekModel);
        }
    }

    //todo background: resubmit selected in background (subscription)
    private async Task ResubmitSelectedMessageAsync(SendMessageModel model)
    {
        if (_selectedMessage == null)
        {
            return;
        }

        //async Task action(CancellationToken cancellationToken) =>
        //    await UserEntityService.ResubmitMessageAsync(TopicName, SubscriptionName,
        //        _selectedMessage,
        //        model.ToSendMessage(),
        //        new ResubmitOptions(model.GenenerateMessageId, model.DeleteMessageAfterResubmit),
        //        cancellationToken);

        //var workItem = new WorkItem($"{TopicName}/{SubscriptionName}", "Send messages", action);
        //WorkerService.Register(workItem);

        CloseOverlaysAndDialogs();

        if (_peekModel != null)
        {
            await PeekAsync(_peekModel);
        }
    }

    //todo background: send selected in background (subscription)
    private async Task SendMessageAsync(SendMessageModel model)
    {
        //async Task action(CancellationToken cancellationToken) =>
        //await UserEntityService.SendMessageAsync(TopicName, [model.ToSendMessage()], cancellationToken);

        //var workItem = new WorkItem($"{TopicName}/{SubscriptionName}", "Send messages", action);
        //WorkerService.Register(workItem);

        CloseOverlaysAndDialogs();

        if (_peekModel != null)
        {
            await PeekAsync(_peekModel);
        }
    }

    //todo background: resubmit all in background (subscription)
    private void ResubmitAllMessages(bool generateMessageIds, bool deleteMesssages)
    {
        //var resubmitOptions = new ResubmitOptions(generateMessageIds, deleteMesssages);
        //async Task action(CancellationToken cancellationToken)
        //    => await UserEntityService.ResubmitAllMessagesAsync(TopicName, SubscriptionName, resubmitOptions, cancellationToken);

        //// todo general: const for action name
        //var workItem = new WorkItem($"{TopicName}/{SubscriptionName}", "Resubmit all dead-letter messages", action);
        //WorkerService.Register(workItem);

        CloseOverlaysAndDialogs();
    }

    //todo background: delete all in background (subscription)
    private void DeleteAllMessages()
    {
        //Func<CancellationToken, Task> action = _source == MessageSource.ActiveMessage
        //    ? async (cancellationToken) => await UserEntityService.DeleteAllActiveMessagesAsync(TopicName, SubscriptionName, cancellationToken)
        //    : async (cancellationToken) => await UserEntityService.DeleteAllDeadLetterMessagesAsync(TopicName, SubscriptionName, cancellationToken);

        //// todo general: const for action name
        //var workItem = new WorkItem($"{TopicName}/{SubscriptionName}", "Delete all messages", action);
        //WorkerService.Register(workItem);

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

