using Droomploeg.Postbode.Aspire.Tests.Infrastructure;
using Droomploeg.Postbode.WebApp.Components.Controls.AzureServiceBus;
using Droomploeg.Postbode.WebApp.Components.Controls.AzureServiceBus.Models;

namespace Droomploeg.Postbode.Aspire.Tests.Pages.SubscriptionDetailPageTests.Features;

/// <summary>
/// Tests for send message functionality on the SubscriptionDetailPage.
/// </summary>
public class SendMessageTests : PostbodeTestBase
{
    private const string TopicName = "test-topic-send";
    private const string SubscriptionName = "sub-send";

    // ===== Happy scenarios =====

    [Fact]
    public async Task Send_PlainTextMessage_Should_SendToTopic()
    {
        var body = Guid.NewGuid().ToString();
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.SendAsync(new SendMessageModel
        {
            Body = body,
            ContentType = "text/plain"
        });

        var messages = await Fixture.PeekSubscriptionMessagesAsync(TopicName, SubscriptionName);
        Assert.Single(messages);
        Assert.Equal(body, messages[0].Body.ToString());
        Assert.Equal("text/plain", messages[0].ContentType);
    }

    [Fact]
    public async Task Send_JsonMessage_Should_SendWithContentType()
    {
        var body = """{"key": "value"}""";
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.SendAsync(new SendMessageModel
        {
            Body = body,
            ContentType = "application/json"
        });

        var messages = await Fixture.PeekSubscriptionMessagesAsync(TopicName, SubscriptionName);
        Assert.Single(messages);
        Assert.Equal(body, messages[0].Body.ToString());
        Assert.Equal("application/json", messages[0].ContentType);
    }

    [Fact]
    public async Task Send_MessageWithSubject_Should_SetSubject()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.SendAsync(new SendMessageModel
        {
            Body = "test body",
            ContentType = "text/plain",
            Subject = "OrderCreated"
        });

        var messages = await Fixture.PeekSubscriptionMessagesAsync(TopicName, SubscriptionName);
        Assert.Single(messages);
        Assert.Equal("OrderCreated", messages[0].Subject);
    }

    [Fact]
    public async Task Send_MessageWithCustomMessageId_Should_SetMessageId()
    {
        var messageId = Guid.NewGuid().ToString();
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.SendAsync(new SendMessageModel
        {
            Body = "test body",
            ContentType = "text/plain",
            MessageId = messageId,
            GenerateMessageId = false
        });

        var messages = await Fixture.PeekSubscriptionMessagesAsync(TopicName, SubscriptionName);
        Assert.Single(messages);
        Assert.Equal(messageId, messages[0].MessageId);
    }

    [Fact]
    public async Task Send_Message_Should_SetUserNameInApplicationProperties()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.SendAsync(new SendMessageModel
        {
            Body = "test body",
            ContentType = "text/plain"
        });

        var messages = await Fixture.PeekSubscriptionMessagesAsync(TopicName, SubscriptionName);
        Assert.Single(messages);
        Assert.Equal("test-user", messages[0].ApplicationProperties["UserName"]?.ToString());
    }

    [Fact]
    public async Task Send_Message_Should_SetCorrelationId()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.SendAsync(new SendMessageModel
        {
            Body = "test body",
            ContentType = "text/plain"
        });

        var messages = await Fixture.PeekSubscriptionMessagesAsync(TopicName, SubscriptionName);
        Assert.Single(messages);
        Assert.NotNull(messages[0].CorrelationId);
        Assert.NotEmpty(messages[0].CorrelationId);
    }

    [Fact]
    public async Task Send_Message_Should_CloseOverlay()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        await actor.OpenSendOverlayAsync();

        Assert.True(actor.HasOpenOverlays);

        await actor.SendAsync(new SendMessageModel
        {
            Body = "test",
            ContentType = "text/plain"
        });

        Assert.False(actor.HasOpenOverlays);
    }

    // ===== Unhappy scenarios =====

    [Fact]
    public async Task Send_EmptyBody_Should_StillSendMessage()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.SendAsync(new SendMessageModel
        {
            Body = "",
            ContentType = "text/plain"
        });

        var messages = await Fixture.PeekSubscriptionMessagesAsync(TopicName, SubscriptionName);
        Assert.Single(messages);
        Assert.Equal("", messages[0].Body.ToString());
    }

    // ===== Audit scenarios =====

    [Fact]
    public async Task Send_Message_Should_CreateAuditEntry()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        AuditLogger.Clear();

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.SendAsync(new SendMessageModel
        {
            Body = "audit test",
            ContentType = "text/plain"
        });

        var entries = AuditLogger.Entries;
        Assert.Single(entries);
        Assert.Equal("SendMessageAsync", entries[0].Action);
    }

    [Fact]
    public async Task Send_MultipleMessages_Should_CreateAuditEntryForEach()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);
        AuditLogger.Clear();

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.SendAsync(new SendMessageModel { Body = "message 1", ContentType = "text/plain" });
        await actor.SendAsync(new SendMessageModel { Body = "message 2", ContentType = "text/plain" });
        await actor.SendAsync(new SendMessageModel { Body = "message 3", ContentType = "text/plain" });

        var entries = AuditLogger.Entries;
        Assert.Equal(3, entries.Count);
        Assert.All(entries, e => Assert.Equal("SendMessageAsync", e.Action));
    }
}
