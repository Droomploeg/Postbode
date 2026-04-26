using Droomploeg.DreamOps.Aspire.Tests.Infrastructure;
using Droomploeg.DreamOps.WebApp.Components.Controls.AzureServiceBus.Models;

namespace Droomploeg.DreamOps.Aspire.Tests.Pages.SubscriptionDetailPageTests.Features;

/// <summary>
/// Tests for selecting a message and verifying the displayed properties on the SubscriptionDetailPage.
/// Flow: send message with known properties, peek, select, verify mapping in ReceivedMessageControl.
/// </summary>
public class SelectMessageTests : DreamOpsTestBase
{
    private const string TopicName = "test-topic-select";
    private const string SubscriptionName = "sub-select";

    // ===== Body =====

    [Fact]
    public async Task SelectMessage_Should_ShowMessageBody()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        await actor.SendAsync(new SendMessageModel { Body = "Hello from topic", ContentType = "text/plain" });

        var messages = await actor.PeekActiveAsync();
        await actor.SelectMessageAsync(messages.First());

        Assert.True(actor.HasMessageBody("Hello from topic"));
    }

    [Fact]
    public async Task SelectMessage_Should_ShowJsonBodyFormatted()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        await actor.SendAsync(new SendMessageModel { Body = """{"order":"123"}""", ContentType = "application/json" });

        var messages = await actor.PeekActiveAsync();
        await actor.SelectMessageAsync(messages.First());

        Assert.True(actor.HasMessageBody("order"));
        Assert.True(actor.HasMessageBody("123"));
    }

    // ===== Broker properties =====

    [Fact]
    public async Task SelectMessage_Should_ShowContentType()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        await actor.SendAsync(new SendMessageModel { Body = "test", ContentType = "application/json" });

        var messages = await actor.PeekActiveAsync();
        await actor.SelectMessageAsync(messages.First());
        actor.SwitchToMessagePropertiesTab();

        Assert.True(actor.HasBrokerProperty("application/json"));
    }

    [Fact]
    public async Task SelectMessage_Should_ShowSubject()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        await actor.SendAsync(new SendMessageModel { Body = "test", ContentType = "text/plain", Subject = "OrderCreated" });

        var messages = await actor.PeekActiveAsync();
        await actor.SelectMessageAsync(messages.First());
        actor.SwitchToMessagePropertiesTab();

        Assert.True(actor.HasBrokerProperty("OrderCreated"));
    }

    [Fact]
    public async Task SelectMessage_Should_ShowMessageId()
    {
        var messageId = Guid.NewGuid().ToString();
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        await actor.SendAsync(new SendMessageModel
        {
            Body = "test",
            ContentType = "text/plain",
            MessageId = messageId,
            GenerateMessageId = false
        });

        var messages = await actor.PeekActiveAsync();
        await actor.SelectMessageAsync(messages.First());
        actor.SwitchToMessagePropertiesTab();

        Assert.True(actor.HasBrokerProperty(messageId));
    }

    [Fact]
    public async Task SelectMessage_Should_ShowCorrelationId()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        await actor.SendAsync(new SendMessageModel { Body = "test", ContentType = "text/plain" });

        var messages = await actor.PeekActiveAsync();
        await actor.SelectMessageAsync(messages.First());
        actor.SwitchToMessagePropertiesTab();

        Assert.True(actor.HasBrokerProperty("Correlation Id"));
    }

    // ===== Custom properties =====

    [Fact]
    public async Task SelectMessage_Should_ShowUserNameInCustomProperties()
    {
        await Fixture.PrepareSubscriptionAsync(TopicName, SubscriptionName);

        var actor = DetailPageActor.Create(this, TopicName, SubscriptionName);
        await actor.SendAsync(new SendMessageModel { Body = "test", ContentType = "text/plain" });

        var messages = await actor.PeekActiveAsync();
        await actor.SelectMessageAsync(messages.First());
        actor.SwitchToMessagePropertiesTab();

        Assert.True(actor.HasCustomProperty("UserName"));
        Assert.True(actor.HasBrokerProperty("test-user"));
    }
}
