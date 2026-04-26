using Droomploeg.DreamOps.Aspire.Tests.Infrastructure;
using Droomploeg.DreamOps.WebApp.Components.Controls.AzureServiceBus.Models;

namespace Droomploeg.DreamOps.Aspire.Tests.Pages.QueueDetailPageTests.Features;

/// <summary>
/// Tests for session-enabled queue operations on the QueueDetailPage.
/// </summary>
public class SessionTests : DreamOpsTestBase
{
    private const string QueueName = "test-queue-session";
    private const string SessionId = "test-session-1";

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        RegisterSessionEntity(QueueName);
    }

    // ===== Happy scenarios =====

    [Fact]
    public async Task Send_MessageWithSessionId_Should_SetSessionId()
    {
        await Fixture.PrepareSessionQueueAsync(QueueName, SessionId);

        var actor = DetailPageActor.Create(this, QueueName);

        await actor.SendAsync(new SendMessageModel
        {
            Body = "session message",
            ContentType = "text/plain",
            SessionId = SessionId
        });

        var messages = await Fixture.PeekActiveMessagesAsync(QueueName);
        Assert.Single(messages);
        Assert.Equal(SessionId, messages[0].SessionId);
    }

    [Fact]
    public async Task Send_MessageWithReplyToSessionId_Should_SetReplyToSessionId()
    {
        await Fixture.PrepareSessionQueueAsync(QueueName, SessionId);

        var actor = DetailPageActor.Create(this, QueueName);

        await actor.SendAsync(new SendMessageModel
        {
            Body = "session reply message",
            ContentType = "text/plain",
            SessionId = SessionId,
            ReplyToSessionId = "reply-session-1"
        });

        var messages = await Fixture.PeekActiveMessagesAsync(QueueName);
        Assert.Single(messages);
        Assert.Equal("reply-session-1", messages[0].ReplyToSessionId);
    }

    [Fact]
    public async Task Peek_SessionQueue_Should_ReturnMessages()
    {
        await Fixture.PrepareSessionQueueAsync(QueueName, SessionId);
        await Fixture.SendMessageAsync(QueueName, "session peek test", sessionId: SessionId);

        var actor = DetailPageActor.Create(this, QueueName);
        var messages = await actor.PeekActiveAsync();

        Assert.Single(messages);
        Assert.Equal(SessionId, messages.First().SessionId);
    }

    [Fact]
    public async Task Delete_SessionMessage_Should_RemoveFromQueue()
    {
        await Fixture.PrepareSessionQueueAsync(QueueName, SessionId);
        await Fixture.SendMessageAsync(QueueName, "session delete test", sessionId: SessionId);

        var actor = DetailPageActor.Create(this, QueueName);
        var messages = await actor.PeekActiveAsync();
        Assert.Single(messages);

        await actor.DeleteAsync(messages.First());

        var remaining = await Fixture.CountActiveMessagesAsync(QueueName);
        Assert.Equal(0, remaining);
    }

    [Fact]
    public async Task DeadLetter_SessionMessage_Should_MoveToDeadLetterQueue()
    {
        await Fixture.PrepareSessionQueueAsync(QueueName, SessionId);
        await Fixture.SendMessageAsync(QueueName, "session DLQ test", sessionId: SessionId);

        var actor = DetailPageActor.Create(this, QueueName);
        var messages = await actor.PeekActiveAsync();
        Assert.Single(messages);

        await actor.DeadLetterAsync(messages.First(), "SessionReason", "Session dead-letter test");

        var activeCount = await Fixture.CountActiveMessagesAsync(QueueName);
        Assert.Equal(0, activeCount);

        var dlqMessages = await Fixture.PeekDeadLetterMessagesAsync(QueueName);
        Assert.Single(dlqMessages);
        Assert.Equal("session DLQ test", dlqMessages[0].Body.ToString());
    }

    // ===== Audit scenarios =====

    [Fact]
    public async Task Send_SessionMessage_Should_CreateAuditEntry()
    {
        await Fixture.PrepareSessionQueueAsync(QueueName, SessionId);
        AuditLogger.Clear();

        var actor = DetailPageActor.Create(this, QueueName);

        await actor.SendAsync(new SendMessageModel
        {
            Body = "audit session test",
            ContentType = "text/plain",
            SessionId = SessionId
        });

        var entries = AuditLogger.Entries;
        Assert.Single(entries);
        Assert.Equal("SendMessageAsync", entries[0].Action);
        Assert.Equal(QueueName, entries[0].Resource);
    }
}
