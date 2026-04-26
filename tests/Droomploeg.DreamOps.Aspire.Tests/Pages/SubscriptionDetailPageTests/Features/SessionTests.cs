using Droomploeg.DreamOps.Aspire.Tests.Infrastructure;
using Droomploeg.DreamOps.WebApp.Components.Controls.AzureServiceBus.Models;

namespace Droomploeg.DreamOps.Aspire.Tests.Pages.SubscriptionDetailPageTests.Features;

/// <summary>
/// Tests for session-enabled subscription operations on the SubscriptionDetailPage.
/// </summary>
public class SessionTests : DreamOpsTestBase
{
    private const string TopicName = "test-topic-session";
    private const string SubscriptionName = "sub-session";
    private const string SessionId = "test-session-1";

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        RegisterSessionEntity(SubscriptionName);
    }

    // ===== Happy scenarios =====

    [Fact]
    public async Task Send_MessageWithSessionId_Should_SetSessionId()
    {
        await Fixture.PrepareSessionSubscriptionAsync(TopicName, SubscriptionName, SessionId);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.SendAsync(new SendMessageModel
        {
            Body = "session message",
            ContentType = "text/plain",
            SessionId = SessionId
        });

        var messages = await Fixture.PeekSubscriptionMessagesAsync(TopicName, SubscriptionName);
        Assert.Single(messages);
        Assert.Equal(SessionId, messages[0].SessionId);
    }

    [Fact]
    public async Task Send_MessageWithReplyToSessionId_Should_SetReplyToSessionId()
    {
        await Fixture.PrepareSessionSubscriptionAsync(TopicName, SubscriptionName, SessionId);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.SendAsync(new SendMessageModel
        {
            Body = "session reply message",
            ContentType = "text/plain",
            SessionId = SessionId,
            ReplyToSessionId = "reply-session-1"
        });

        var messages = await Fixture.PeekSubscriptionMessagesAsync(TopicName, SubscriptionName);
        Assert.Single(messages);
        Assert.Equal("reply-session-1", messages[0].ReplyToSessionId);
    }

    [Fact]
    public async Task Peek_SessionSubscription_Should_ReturnMessages()
    {
        await Fixture.PrepareSessionSubscriptionAsync(TopicName, SubscriptionName, SessionId);
        await Fixture.SendTopicMessageAsync(TopicName, "session peek test", sessionId: SessionId);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekActiveAsync();

        Assert.Single(messages);
        Assert.Equal(SessionId, messages.First().SessionId);
    }

    [Fact]
    public async Task Delete_SessionMessage_Should_RemoveFromSubscription()
    {
        await Fixture.PrepareSessionSubscriptionAsync(TopicName, SubscriptionName, SessionId);
        await Fixture.SendTopicMessageAsync(TopicName, "session delete test", sessionId: SessionId);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekActiveAsync();
        Assert.Single(messages);

        await actor.DeleteAsync(messages.First());

        var remaining = await Fixture.CountSubscriptionActiveMessagesAsync(TopicName, SubscriptionName);
        Assert.Equal(0, remaining);
    }

    [Fact]
    public async Task DeadLetter_SessionMessage_Should_MoveToDeadLetterQueue()
    {
        await Fixture.PrepareSessionSubscriptionAsync(TopicName, SubscriptionName, SessionId);
        await Fixture.SendTopicMessageAsync(TopicName, "session DLQ test", sessionId: SessionId);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        var messages = await actor.PeekActiveAsync();
        Assert.Single(messages);

        await actor.DeadLetterAsync(messages.First(), "SessionReason", "Session dead-letter test");

        var activeCount = await Fixture.CountSubscriptionActiveMessagesAsync(TopicName, SubscriptionName);
        Assert.Equal(0, activeCount);

        var dlqMessages = await Fixture.PeekSubscriptionDeadLetterMessagesAsync(TopicName, SubscriptionName);
        Assert.Single(dlqMessages);
        Assert.Equal("session DLQ test", dlqMessages[0].Body.ToString());
    }

    // ===== Audit scenarios =====

    [Fact]
    public async Task Send_SessionMessage_Should_CreateAuditEntry()
    {
        await Fixture.PrepareSessionSubscriptionAsync(TopicName, SubscriptionName, SessionId);
        AuditLogger.Clear();

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);

        await actor.SendAsync(new SendMessageModel
        {
            Body = "audit session test",
            ContentType = "text/plain",
            SessionId = SessionId
        });

        var entries = AuditLogger.Entries;
        Assert.Single(entries);
        Assert.Equal("SendMessageAsync", entries[0].Action);
    }
}
