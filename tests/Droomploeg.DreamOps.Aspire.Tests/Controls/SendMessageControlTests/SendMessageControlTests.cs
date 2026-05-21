using AngleSharp.Dom;
using Bunit;
using Droomploeg.DreamOps.Aspire.Tests.Infrastructure;
using Droomploeg.DreamOps.WebApp.Components.Controls.AzureServiceBus;

namespace Droomploeg.DreamOps.Aspire.Tests.Controls.SendMessageControlTests;

public class SendMessageControlTests : DreamOpsTestBase
{
    [Fact]
    public void Send_Should_BeHidden_WhenBodyIsEmpty()
    {
        var cut = RenderComponent<SendMessageControl>();

        Assert.Empty(cut.FindAll("button.primary"));
    }

    [Fact]
    public void Send_Should_AppearAfterBodyIsFilled()
    {
        var cut = RenderComponent<SendMessageControl>();

        cut.Find("#Body").Change("hello world");

        Assert.Single(cut.FindAll("button.primary"));
    }

    [Fact]
    public void Send_Should_BeHidden_WhenSessionEnabledAndSessionIdIsEmpty()
    {
        var cut = RenderComponent<SendMessageControl>(p => p
            .Add(c => c.SessionEnabled, true));

        cut.Find("#Body").Change("hello world");

        Assert.Empty(cut.FindAll("button.primary"));
    }

    [Fact]
    public void Send_Should_AppearWhenSessionEnabledAndAllRequiredFieldsAreFilled()
    {
        var cut = RenderComponent<SendMessageControl>(p => p
            .Add(c => c.SessionEnabled, true));

        cut.Find("#Body").Change("hello world");
        FindSessionIdInput(cut).Change("my-session");

        Assert.Single(cut.FindAll("button.primary"));
    }

    [Fact]
    public void SessionIdRequiredError_Should_AppearWhenSessionEnabledAndSessionIdIsEmpty()
    {
        var cut = RenderComponent<SendMessageControl>(p => p
            .Add(c => c.SessionEnabled, true));

        Assert.Contains("Session Id is required", cut.Markup);
    }

    [Fact]
    public void SessionIdRequiredError_Should_DisappearAfterSessionIdIsFilled()
    {
        var cut = RenderComponent<SendMessageControl>(p => p
            .Add(c => c.SessionEnabled, true));

        FindSessionIdInput(cut).Change("my-session");

        Assert.DoesNotContain("Session Id is required", cut.Markup);
    }

    [Fact]
    public void SessionIdRequiredError_Should_NotAppearWhenSessionDisabled()
    {
        var cut = RenderComponent<SendMessageControl>();

        Assert.DoesNotContain("Session Id is required", cut.Markup);
    }

    private static IElement FindSessionIdInput(IRenderedComponent<SendMessageControl> cut)
    {
        var sessionRow = cut.FindAll("tr").First(r => r.TextContent.Contains("Session Id (required)"));
        return sessionRow.QuerySelector("input")
            ?? throw new InvalidOperationException("Session Id input not found.");
    }
}
