using Droomploeg.Postbode.Aspire.Tests.Infrastructure;
using Droomploeg.Postbode.WebApp.Components.Controls.AzureServiceBus.Models;
using Microsoft.Extensions.Configuration;

namespace Droomploeg.Postbode.Aspire.Tests.Pages.QueueDetailPageTests.Features;

/// <summary>
/// Tests for the Application Insights links on message identifiers in the ReceivedMessageControl.
/// Message identifiers are set by whoever sends the message, so they must be rendered as text,
/// never as markup.
/// </summary>
public class MessageLinkTests : PostbodeTestBase
{
    private const string QueueName = "test-message-link";
    private const string MaliciousMessageId = "\"><img src=x onerror=alert(1)>";

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApplicationInsightLink:AzureDirectoryName"] = "contoso.onmicrosoft.com",
                ["ApplicationInsightLink:SubscriptionId"] = "00000000-0000-0000-0000-000000000000",
                ["ApplicationInsightLink:ResourceGroupName"] = "test-rg",
                ["ApplicationInsightLink:ResourceName"] = "test-ai",
            })
            .Build();
        Services.AddSingleton<IConfiguration>(configuration);
    }

    [Fact]
    public async Task SelectMessage_Should_ShowMessageIdAsApplicationInsightsLink()
    {
        var messageId = Guid.NewGuid().ToString();
        await Fixture.PrepareQueueAsync(QueueName);

        var actor = DetailPageActor.Create(this, QueueName);
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

        Assert.True(actor.HasPropertyLink(messageId));
        Assert.StartsWith("https://portal.azure.com/", actor.GetPropertyLinkAttribute(messageId, "href"));
        Assert.Equal("_blank", actor.GetPropertyLinkAttribute(messageId, "target"));
        Assert.Equal("noopener noreferrer", actor.GetPropertyLinkAttribute(messageId, "rel"));
    }

    [Fact]
    public async Task SelectMessage_Should_RenderMaliciousMessageIdAsText()
    {
        await Fixture.PrepareQueueAsync(QueueName);

        var actor = DetailPageActor.Create(this, QueueName);
        await actor.SendAsync(new SendMessageModel
        {
            Body = "test",
            ContentType = "text/plain",
            MessageId = MaliciousMessageId,
            GenerateMessageId = false
        });

        var messages = await actor.PeekActiveAsync();
        await actor.SelectMessageAsync(messages.First());
        actor.SwitchToMessagePropertiesTab();

        Assert.False(actor.HasElement("img"));

        Assert.True(actor.HasPropertyLink(MaliciousMessageId));
        var href = actor.GetPropertyLinkAttribute(MaliciousMessageId, "href");
        Assert.NotNull(href);
        Assert.DoesNotContain("\"", href);
        Assert.DoesNotContain("<", href);
    }
}
