using Droomploeg.Postbode.Aspire.Tests.Infrastructure;

namespace Droomploeg.Postbode.Aspire.Tests.Pages.QueueDetailPageTests.Features;

public class PeekMessagesTests : PostbodeTestBase
{
    private const string QueueName = "test-peek-active";

    // ===== Happy scenarios - Active messages =====

    [Fact]
    public async Task PeekActive_WithMessages_Should_ReturnMessages()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendMessageAsync(QueueName, "Message 1");
        await Fixture.SendMessageAsync(QueueName, "Message 2");

        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        var messages = await actor.PeekActiveAsync();

        // Assert
        Assert.Equal(2, messages.Count);
    }

    [Fact]
    public async Task PeekActive_EmptyQueue_Should_ReturnEmpty()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);

        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        var messages = await actor.PeekActiveAsync();

        // Assert
        Assert.Empty(messages);
    }

    [Fact]
    public async Task PeekActive_Should_ReturnMessageContent()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendMessageAsync(QueueName, "Expected body content");

        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        var messages = await actor.PeekActiveAsync();

        // Assert
        Assert.Single(messages);
        Assert.Equal("Expected body content", messages.First().Body.ToString());
    }

    [Fact]
    public async Task PeekActive_WithLimit_Should_RespectNumberOfMessages()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        for (var i = 0; i < 5; i++)
        {
            await Fixture.SendMessageAsync(QueueName, $"Message {i}");
        }

        var actor = DetailPageActor.Create(this, QueueName);

        // Act — only peek 2
        var messages = await actor.PeekActiveAsync(numberOfMessages: 2);

        // Assert
        Assert.Equal(2, messages.Count);
    }

    // ===== Happy scenarios - Dead-letter messages =====

    [Fact]
    public async Task PeekDeadLetter_WithMessages_Should_ReturnMessages()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendAndDeadLetterMessageAsync(QueueName, "DLQ message 1");
        await Fixture.SendAndDeadLetterMessageAsync(QueueName, "DLQ message 2");

        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        var messages = await actor.PeekDeadLetterAsync();

        // Assert
        Assert.Equal(2, messages.Count);
    }

    [Fact]
    public async Task PeekDeadLetter_EmptyQueue_Should_ReturnEmpty()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);

        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        var messages = await actor.PeekDeadLetterAsync();

        // Assert
        Assert.Empty(messages);
    }

    [Fact]
    public async Task PeekDeadLetter_Should_ReturnMessageContent()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendAndDeadLetterMessageAsync(QueueName, "Dead letter body");

        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        var messages = await actor.PeekDeadLetterAsync();

        // Assert
        Assert.Single(messages);
        Assert.Equal("Dead letter body", messages.First().Body.ToString());
    }

    // ===== Unhappy scenarios =====

    [Fact]
    public async Task PeekActive_AfterAllMessagesDeleted_Should_ReturnEmpty()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendMessageAsync(QueueName, "Temporary message");

        var actor = DetailPageActor.Create(this, QueueName);
        var messages = await actor.PeekActiveAsync();
        Assert.Single(messages);

        // Delete the message outside the page
        await Fixture.PrepareQueueAsync(QueueName);

        // Act — peek again
        messages = await actor.PeekActiveAsync();

        // Assert
        Assert.Empty(messages);
    }

    // ===== Audit scenarios =====

    [Fact]
    public async Task PeekActive_Should_CreateAuditEntry()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendMessageAsync(QueueName, "Audit peek test");
        AuditLogger.Clear();

        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.PeekActiveAsync();

        // Assert
        var peekEntries = AuditLogger.Entries
            .Where(e => e.Action == "PeekActiveMessagesAsync")
            .ToList();
        Assert.Single(peekEntries);
        Assert.Equal(QueueName, peekEntries[0].Resource);
    }

    [Fact]
    public async Task PeekDeadLetter_Should_CreateAuditEntry()
    {
        // Arrange
        await Fixture.PrepareQueueAsync(QueueName);
        await Fixture.SendAndDeadLetterMessageAsync(QueueName, "Audit DLQ test");
        AuditLogger.Clear();

        var actor = DetailPageActor.Create(this, QueueName);

        // Act
        await actor.PeekDeadLetterAsync();

        // Assert
        var peekEntries = AuditLogger.Entries
            .Where(e => e.Action == "PeekDeadLetterMessagesAsync")
            .ToList();
        Assert.Single(peekEntries);
        Assert.Equal(QueueName, peekEntries[0].Resource);
    }
}
