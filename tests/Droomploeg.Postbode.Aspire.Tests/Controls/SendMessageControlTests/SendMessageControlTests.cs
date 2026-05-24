using AngleSharp.Dom;
using Bunit;
using Droomploeg.Postbode.Aspire.Tests.Infrastructure;
using Droomploeg.Postbode.WebApp.Components.Controls.AzureServiceBus;

namespace Droomploeg.Postbode.Aspire.Tests.Controls.SendMessageControlTests;

public class SendMessageControlTests : PostbodeTestBase
{
    [Fact]
    public void Send_Should_BeHidden_WhenBodyIsEmpty()
    {
        var cut = Render<SendMessageControl>();

        Assert.Empty(cut.FindAll("button.primary"));
    }

    [Fact]
    public void Send_Should_AppearAfterBodyIsFilled()
    {
        var cut = Render<SendMessageControl>();

        cut.Find("#Body").Change("hello world");

        Assert.Single(cut.FindAll("button.primary"));
    }

    [Fact]
    public void Send_Should_BeHidden_WhenSessionEnabledAndSessionIdIsEmpty()
    {
        var cut = Render<SendMessageControl>(p => p
            .Add(c => c.SessionEnabled, true));

        cut.Find("#Body").Change("hello world");

        Assert.Empty(cut.FindAll("button.primary"));
    }

    [Fact]
    public void Send_Should_AppearWhenSessionEnabledAndAllRequiredFieldsAreFilled()
    {
        var cut = Render<SendMessageControl>(p => p
            .Add(c => c.SessionEnabled, true));

        cut.Find("#Body").Change("hello world");
        FindSessionIdInput(cut).Change("my-session");

        Assert.Single(cut.FindAll("button.primary"));
    }

    [Fact]
    public void SessionIdRequiredError_Should_AppearWhenSessionEnabledAndSessionIdIsEmpty()
    {
        var cut = Render<SendMessageControl>(p => p
            .Add(c => c.SessionEnabled, true));

        Assert.Contains("Session Id is required", cut.Markup);
    }

    [Fact]
    public void SessionIdRequiredError_Should_DisappearAfterSessionIdIsFilled()
    {
        var cut = Render<SendMessageControl>(p => p
            .Add(c => c.SessionEnabled, true));

        FindSessionIdInput(cut).Change("my-session");

        Assert.DoesNotContain("Session Id is required", cut.Markup);
    }

    [Fact]
    public void SessionIdRequiredError_Should_NotAppearWhenSessionDisabled()
    {
        var cut = Render<SendMessageControl>();

        Assert.DoesNotContain("Session Id is required", cut.Markup);
    }

    private static IElement FindSessionIdInput(IRenderedComponent<SendMessageControl> cut)
    {
        var sessionRow = cut.FindAll("tr").First(r => r.TextContent.Contains("Session Id (required)"));
        return sessionRow.QuerySelector("input")
            ?? throw new InvalidOperationException("Session Id input not found.");
    }
}
