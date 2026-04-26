using Azure.Messaging.ServiceBus;
using Bunit;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Droomploeg.DreamOps.WebApp.Components.Controls.AzureServiceBus;
using Droomploeg.DreamOps.WebApp.Components.Controls.AzureServiceBus.Models;
using Droomploeg.DreamOps.WebApp.Components.Pages;
using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.Aspire.Tests.Actors;

public static class DetailPageActor
{
    public static DetailPageActor<QueueDetailPage> Create(TestContext context, string queueName)
    {
        var page = context.RenderComponent<QueueDetailPage>(p =>
            p.Add(x => x.QueueName, queueName));
        return new DetailPageActor<QueueDetailPage>(page);
    }

    public static DetailPageActor<SubscriptionDetailPage> Create(
        TestContext context, string topicName, string subscriptionName)
    {
        var page = context.RenderComponent<SubscriptionDetailPage>(p => p
            .Add(x => x.TopicName, topicName)
            .Add(x => x.SubscriptionName, subscriptionName));
        return new DetailPageActor<SubscriptionDetailPage>(page);
    }
}

public class DetailPageActor<TPage> where TPage : IComponent
{
    private readonly IRenderedComponent<TPage> _page;

    public DetailPageActor(IRenderedComponent<TPage> page)
    {
        _page = page;
        _page.WaitForState(() => _page.Markup.Contains("send-button"), TimeSpan.FromSeconds(5));
        _page.WaitForState(() => !_page.Markup.Contains("Loading..."), TimeSpan.FromSeconds(5));
    }

    public bool HasOpenOverlays =>
        _page.FindAll("offcanvas").Any(e => e.ClassList.Contains("open"));

    public bool HasVisibleDialogs =>
        _page.FindAll(".dialog-container").Any();

    public bool HasConfirmButton =>
        _page.FindAll("button.primary").Any(b => b.TextContent == "Confirm");

    public bool HasMessageBody(string text) =>
        _page.FindAll(".body").Any(e => e.TextContent.Contains(text));

    public bool HasBrokerProperty(string value) =>
        _page.Markup.Contains(value);

    public bool HasCustomProperty(string key) =>
        _page.Markup.Contains(key);

    public async Task SelectMessageAsync(ServiceBusReceivedMessage message)
    {
        var peekControl = _page.FindComponent<PeekControl>();
        await _page.InvokeAsync(() => peekControl.Instance.OnSelected.InvokeAsync(message));
    }

    public void SwitchToMessagePropertiesTab()
    {
        var tab = _page.FindAll("tabheader")
            .First(t => t.TextContent.Contains("Message Properties"));
        tab.Click();
    }

    public async Task OpenSendOverlayAsync()
    {
        _page.Find(".send-button").Click();
    }

    public async Task<ICollection<ServiceBusReceivedMessage>> PeekActiveAsync(
        int startIndex = 0, int numberOfMessages = 50)
    {
        var peekModel = new PeekModel { StartIndex = startIndex, NumberOfMessages = numberOfMessages };
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);

        while (DateTime.UtcNow < deadline)
        {
            var peekControl = _page.FindComponent<PeekControl>();
            var previousMessages = peekControl.Instance.Messages;
            await _page.InvokeAsync(() => peekControl.Instance.OnPeek.InvokeAsync(peekModel));

            _page.WaitForState(
                () => peekControl.Instance.Messages != previousMessages,
                TimeSpan.FromSeconds(5));

            var messages = peekControl.Instance.Messages ?? [];
            if (messages.Count > 0)
            {
                return messages;
            }

            await Task.Delay(250);
        }

        return [];
    }

    public async Task SwitchToDeadLetterTabAsync()
    {
        var peekControl = _page.FindComponent<PeekControl>();
        await _page.InvokeAsync(() => peekControl.Instance.OnMessageSourceChanged.InvokeAsync(MessageSource.DeadLetterMessage));
    }

    public async Task<ICollection<ServiceBusReceivedMessage>> PeekDeadLetterAsync(
        int startIndex = 0, int numberOfMessages = 50)
    {
        await SwitchToDeadLetterTabAsync();

        var peekControl = _page.FindComponent<PeekControl>();
        var previousMessages = peekControl.Instance.Messages;
        var peekModel = new PeekModel { StartIndex = startIndex, NumberOfMessages = numberOfMessages };
        await _page.InvokeAsync(() => peekControl.Instance.OnPeek.InvokeAsync(peekModel));

        _page.WaitForState(
            () => peekControl.Instance.Messages != previousMessages,
            TimeSpan.FromSeconds(5));

        return peekControl.Instance.Messages ?? [];
    }

    public async Task DeleteAsync(ServiceBusReceivedMessage message)
    {
        var peekControl = _page.FindComponent<PeekControl>();
        await _page.InvokeAsync(() => peekControl.Instance.OnDelete.InvokeAsync(message));

        _page.WaitForState(
            () => _page.FindAll(".dialog-container").Any(),
            TimeSpan.FromSeconds(2));

        var confirmButton = _page.Find(".dialog-container button.primary");
        await confirmButton.ClickAsync(new());

        _page.WaitForState(
            () => !_page.FindAll(".dialog-container").Any(),
            TimeSpan.FromSeconds(2));
    }

    public async Task DeleteAllActiveAsync()
    {
        var peekControl = _page.FindComponent<PeekControl>();
        await _page.InvokeAsync(() => peekControl.Instance.OnDeleteAll.InvokeAsync());

        _page.WaitForState(
            () => _page.FindAll(".dialog-container").Any(),
            TimeSpan.FromSeconds(2));

        var confirmButton = _page.Find(".dialog-container button.primary");
        await confirmButton.ClickAsync(new());

        _page.WaitForState(
            () => !_page.FindAll(".dialog-container").Any(),
            TimeSpan.FromSeconds(2));
    }

    public async Task DeleteAllDeadLetterAsync()
    {
        await SwitchToDeadLetterTabAsync();

        var peekControl = _page.FindComponent<PeekControl>();
        await _page.InvokeAsync(() => peekControl.Instance.OnDeleteAll.InvokeAsync());

        _page.WaitForState(
            () => _page.FindAll(".dialog-container").Any(),
            TimeSpan.FromSeconds(2));

        var confirmButton = _page.Find(".dialog-container button.primary");
        await confirmButton.ClickAsync(new());

        _page.WaitForState(
            () => !_page.FindAll(".dialog-container").Any(),
            TimeSpan.FromSeconds(2));
    }

    public async Task DeadLetterAsync(
        ServiceBusReceivedMessage message, string reason, string description)
    {
        var peekControl = _page.FindComponent<PeekControl>();
        await _page.InvokeAsync(() => peekControl.Instance.OnDeadLetter.InvokeAsync(message));

        var dlControl = _page.FindComponent<DeadLetterMessageControl>();
        var model = new DeadLetterMessageModel { Reason = reason, Description = description };
        await _page.InvokeAsync(() => dlControl.Instance.OnSend.InvokeAsync(model));

        WaitForNoOpenOverlays();
    }

    public async Task SendAsync(SendMessageModel model)
    {
        _page.Find(".send-button").Click();
        var sendControl = _page.FindComponent<SendMessageControl>();
        await _page.InvokeAsync(() => sendControl.Instance.OnSend.InvokeAsync(model));

        WaitForNoOpenOverlays();
    }

    public async Task ResubmitAsync(ServiceBusReceivedMessage message, SendMessageModel model)
    {
        var peekControl = _page.FindComponent<PeekControl>();
        await _page.InvokeAsync(() => peekControl.Instance.OnResubmit.InvokeAsync(message));

        var sendControls = _page.FindComponents<SendMessageControl>();
        var resubmitControl = sendControls.First(c => c.Instance.Message is not null);

        await _page.InvokeAsync(() => resubmitControl.Instance.OnSend.InvokeAsync(model));

        WaitForNoOpenOverlays();
    }

    public async Task ResubmitAllAsync(bool generateMessageIds = false, bool deleteMessages = false)
    {
        await SwitchToDeadLetterTabAsync();

        var peekControl = _page.FindComponent<PeekControl>();
        await _page.InvokeAsync(() => peekControl.Instance.OnResubmitAll.InvokeAsync());

        var resubmitControl = _page.FindComponent<ResubmitAllMessageControl>();
        var args = (GenerateMessageIds: generateMessageIds, DeleteMessages: deleteMessages);
        await _page.InvokeAsync(() => resubmitControl.Instance.OnSend.InvokeAsync(args));

        WaitForNoOpenOverlays();
    }

    private void WaitForNoOpenOverlays()
    {
        _page.WaitForState(
            () => !_page.FindAll("offcanvas").Any(e => e.ClassList.Contains("open")),
            TimeSpan.FromSeconds(2));
    }
}
