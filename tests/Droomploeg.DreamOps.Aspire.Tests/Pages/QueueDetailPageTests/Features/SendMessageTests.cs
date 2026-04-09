using Droomploeg.DreamOps.Aspire.Tests.Infrastructure;
using Droomploeg.DreamOps.WebApp.Components.Controls.AzureServiceBus;
using Droomploeg.DreamOps.WebApp.Components.Controls.AzureServiceBus.Models;

namespace Droomploeg.DreamOps.Aspire.Tests.Pages.QueueDetailPageTests.Features;

public class SendMessageTests : DreamOpsTestBase
{
    private const string QueueName = "test-send-message";

    // ===== Happy scenarios =====

    [Fact]
    public async Task Send_PlainTextMessage_Should_QueueMessage()
    {
        // Arrange
        var body = Guid.NewGuid().ToString();
        await Fixture.PrepareQueueAsync(QueueName);
        AuditLogger.Clear();

        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.SendAsync(new SendMessageModel
        {
            Body = body,
            ContentType = "text/plain"
        });

        // Assert
        var messages = await Fixture.PeekActiveMessagesAsync(QueueName);
        Assert.Single(messages);
        Assert.Equal(body, messages[0].Body.ToString());
        Assert.Equal("text/plain", messages[0].ContentType);
    }

    [Fact]
    public async Task Send_JsonMessage_Should_QueueMessageWithContentType()
    {
        // Arrange
        var body = """{"key": "value"}""";
        await Fixture.PrepareQueueAsync(QueueName);

        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.SendAsync(new SendMessageModel
        {
            Body = body,
            ContentType = "application/json"
        });

        // Assert
        var messages = await Fixture.PeekActiveMessagesAsync(QueueName);
        Assert.Single(messages);
        Assert.Equal(body, messages[0].Body.ToString());
        Assert.Equal("application/json", messages[0].ContentType);
    }

    [Fact]
    public async Task Send_MessageWithSubject_Should_SetSubject()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);

        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.SendAsync(new SendMessageModel
        {
            Body = "test body",
            ContentType = "text/plain",
            Subject = "OrderCreated"
        });

        // Assert
        var messages = await Fixture.PeekActiveMessagesAsync(QueueName);
        Assert.Single(messages);
        Assert.Equal("OrderCreated", messages[0].Subject);
    }

    [Fact]
    public async Task Send_MessageWithCustomMessageId_Should_SetMessageId()
    {
        // Arrange
        var messageId = Guid.NewGuid().ToString();
        await Fixture.PrepareQueueAsync(QueueName);

        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.SendAsync(new SendMessageModel
        {
            Body = "test body",
            ContentType = "text/plain",
            MessageId = messageId,
            GenerateMessageId = false
        });

        // Assert
        var messages = await Fixture.PeekActiveMessagesAsync(QueueName);
        Assert.Single(messages);
        Assert.Equal(messageId, messages[0].MessageId);
    }

    [Fact]
    public async Task Send_Message_Should_SetUserNameInApplicationProperties()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);

        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.SendAsync(new SendMessageModel
        {
            Body = "test body",
            ContentType = "text/plain"
        });

        // Assert — the ActiveQueueAdapter sets UserName from ApplicationContext
        var messages = await Fixture.PeekActiveMessagesAsync(QueueName);
        Assert.Single(messages);
        Assert.Equal("test-user", messages[0].ApplicationProperties["UserName"]?.ToString());
    }

    [Fact]
    public async Task Send_Message_Should_SetCorrelationId()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);

        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.SendAsync(new SendMessageModel
        {
            Body = "test body",
            ContentType = "text/plain"
        });

        // Assert — the ActiveQueueAdapter sets CorrelationId from ApplicationContext
        var messages = await Fixture.PeekActiveMessagesAsync(QueueName);
        Assert.Single(messages);
        Assert.NotNull(messages[0].CorrelationId);
        Assert.NotEmpty(messages[0].CorrelationId);
    }

    [Fact]
    public async Task Send_Message_Should_CloseOverlay()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);

        var actor = DetailPageActor.Create(this, QueueName);
        await actor.OpenSendOverlayAsync();

        // Verify overlay is open
        Assert.True(actor.HasOpenOverlays);

        // Act
        await actor.SendAsync(new SendMessageModel
        {
            Body = "test",
            ContentType = "text/plain"
        });

        // Assert — overlay should be closed
        Assert.False(actor.HasOpenOverlays);
    }

    // ===== Unhappy scenarios =====

    [Fact]
    public async Task Send_EmptyBody_Should_StillQueueMessage()
    {
        // Arrange — Service Bus allows empty body messages
        await Fixture.PrepareQueueAsync(QueueName);

        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.SendAsync(new SendMessageModel
        {
            Body = "",
            ContentType = "text/plain"
        });

        // Assert
        var messages = await Fixture.PeekActiveMessagesAsync(QueueName);
        Assert.Single(messages);
        Assert.Equal("", messages[0].Body.ToString());
    }

    // ===== Audit scenarios =====

    [Fact]
    public async Task Send_Message_Should_CreateAuditEntry()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        AuditLogger.Clear();

        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.SendAsync(new SendMessageModel
        {
            Body = "audit test",
            ContentType = "text/plain"
        });

        // Assert — audit logger should have recorded the send action
        var entries = AuditLogger.Entries;
        Assert.Single(entries);
        Assert.Equal("SendMessageAsync", entries[0].Action);
        Assert.Equal(QueueName, entries[0].Resource);
    }

    [Fact]
    public async Task Send_MultipleMessages_Should_CreateAuditEntryForEach()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        AuditLogger.Clear();

        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.SendAsync(new SendMessageModel { Body = "message 1", ContentType = "text/plain" });
        await actor.SendAsync(new SendMessageModel { Body = "message 2", ContentType = "text/plain" });
        await actor.SendAsync(new SendMessageModel { Body = "message 3", ContentType = "text/plain" });

        // Assert
        var entries = AuditLogger.Entries;
        Assert.Equal(3, entries.Count);
        Assert.All(entries, e =>
        {
            Assert.Equal("SendMessageAsync", e.Action);
            Assert.Equal(QueueName, e.Resource);
        });
    }
}
