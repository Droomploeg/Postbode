using Azure.Messaging.ServiceBus;
using Droomploeg.DreamOps.Core.Models;
using Droomploeg.DreamOps.Infrastructure.HostedServices.WorkerService;
using Droomploeg.DreamOps.WebApp.Components.Controls.AzureServiceBus.Models;
using Microsoft.AspNetCore.Components;

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

    private Subscription? _subscription;

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
        _subscription = await ServiceBusService.GetSubscriptionAsync(TopicName, SubscriptionName);
        _receivedMessages = [];
        _selectedMessage = null;
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

        if (_source == MessageSource.ActiveMessage)
        {
            _receivedMessages = null;
            _receivedMessages = await EntityService.PeekAsync(TopicName, SubscriptionName, args.StartIndex, args.NumberOfMessages);
        }
        else if (_source == MessageSource.DeadLetterMessage)
        {
            _receivedMessages = null;
            _receivedMessages = await EntityService.PeekDeadletterAsync(TopicName, SubscriptionName, args.StartIndex, args.NumberOfMessages);
        }

        _subscription = await ServiceBusService.GetSubscriptionAsync(TopicName, SubscriptionName);
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

        await EntityService.DeadLetterMessageAsync(TopicName, SubscriptionName, _selectedMessage, ApplicationConstants.ApplicationName, reason, description);

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
            await EntityService.DeleteActiveMessageAsync(TopicName, SubscriptionName, _selectedMessage, default);
        }
        if (_source == MessageSource.DeadLetterMessage)
        {
            await EntityService.DeleteDeadletterMessageAsync(TopicName, SubscriptionName, _selectedMessage, default);
        }

        _selectedMessage = null;
        CloseOverlaysAndDialogs();

        if (_peekModel != null)
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

        async Task action(CancellationToken cancellationToken) =>
            await EntityService.ResubmitDeadletterMessageAsync(TopicName, SubscriptionName,
                _selectedMessage,
                model.ToSendMessage(),
                new ResubmitOptions(model.GenenerateMessageId, model.DeleteMessageAfterResubmit),
                cancellationToken);

        var workItem = new WorkItem($"{TopicName}/{SubscriptionName}", "Send messages", action);
        WorkerService.Register(workItem);

        CloseOverlaysAndDialogs();

        if (_peekModel != null)
        {
            await PeekAsync(_peekModel);
        }
    }

    private async Task SendMessageAsync(SendMessageModel model)
    {
        async Task action(CancellationToken cancellationToken) =>
        await EntityService.SendMessageAsync(TopicName, [model.ToSendMessage()], cancellationToken);

        var workItem = new WorkItem($"{TopicName}/{SubscriptionName}", "Send messages", action);
        WorkerService.Register(workItem);

        CloseOverlaysAndDialogs();

        if (_peekModel != null)
        {
            await PeekAsync(_peekModel);
        }
    }

    private void ResubmitAllMessages(bool generateMessageIds, bool deleteMesssages)
    {
        var resubmitOptions = new ResubmitOptions(generateMessageIds, deleteMesssages);
        async Task action(CancellationToken cancellationToken)
            => await EntityService.ResubmitAllDeadletterMessagesAsync(TopicName, SubscriptionName, resubmitOptions, cancellationToken);

        // todo: const for action name
        var workItem = new WorkItem($"{TopicName}/{SubscriptionName}", "Resubmit all dead-letter messages", action);
        WorkerService.Register(workItem);

        CloseOverlaysAndDialogs();
    }

    private void DeleteAllMessages()
    {
        Func<CancellationToken, Task> action = _source == MessageSource.ActiveMessage
            ? async (cancellationToken) => await EntityService.DeleteAllActiveMessagesAsync(TopicName, SubscriptionName, cancellationToken)
            : async (cancellationToken) => await EntityService.DeleteAllDeadLetterMessagesAsync(TopicName, SubscriptionName, cancellationToken);

        // todo: const for action name
        var workItem = new WorkItem($"{TopicName}/{SubscriptionName}", "Delete all messages", action);
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

