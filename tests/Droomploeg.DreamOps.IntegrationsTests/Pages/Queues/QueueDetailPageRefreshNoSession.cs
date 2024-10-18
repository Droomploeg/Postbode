using Azure.Messaging.ServiceBus;
using Bunit;
using Droomploeg.DreamOps.IntegrationsTests.Common.BUnit;
using Droomploeg.DreamOps.IntegrationsTests.Common.ServiceBus;
using Droomploeg.DreamOps.IntegrationsTests.Pages.Queues.Contants;
using Droomploeg.DreamOps.WebApp.Components.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Droomploeg.DreamOps.IntegrationsTests.Pages.Queues;


[CollectionDefinition(ServicebusEntityNames.QueueDetailRefreshNoSession, DisableParallelization = true)]
public class QueueDetailPageRefreshNoSession : TestContext
{
    private const string ActiveTabHeaderId = "activeMessageCount";
    private const string DeadLetterTabHeaderId = "deadLetterMessageCount";

    [Fact(DisplayName = "Overview Detail Page without message should have 0 on active and dead-letter tabs header")]
    public async Task QueueDetailPageOnLoadShouldHaveZeroMessageCountInQueueAndDeadletterQueue()
    {
        Application.Setup(this, out var clientName);

        var queueTestContext = Services.GetRequiredService<QueueTestContext>();
        await queueTestContext.ClearAll(ServicebusEntityNames.QueueDetailRefreshNoSession);

        var page = RenderComponent<QueueDetailPage>(p => p.Add(p => p.QueueName, ServicebusEntityNames.QueueDetailRefreshNoSession));

        var activeMessageCountTabHeader = page.FindById(ActiveTabHeaderId);
        activeMessageCountTabHeader.TextContent.Should().Be(QueueDetailPageLabel.ActiveTabHeader(0));

        var deadLetterMessageCountTabHeader = page.FindById(DeadLetterTabHeaderId);
        deadLetterMessageCountTabHeader.TextContent.Should().Be(QueueDetailPageLabel.ActiveDeadLetterTabHeader(0));
    }

    [Fact(DisplayName = "Overview Detail Page should on refesh update the counter on active tab")]
    public async Task QueueDetailPageOnLoadShouldHaveOneMessageCountInActiveQueueCount()
    {
        Application.Setup(this, out var clientName);

        var queueTestContext = Services.GetRequiredService<QueueTestContext>();
        await queueTestContext.ClearAll(ServicebusEntityNames.QueueDetailRefreshNoSession);

        var page = RenderComponent<QueueDetailPage>(p => p.Add(p => p.QueueName, ServicebusEntityNames.QueueDetailRefreshNoSession));
        await queueTestContext.SendAsync(ServicebusEntityNames.QueueDetailRefreshNoSession, new ServiceBusMessage("test message"));

        var activeMessageCountTabHeader = page.FindById(ActiveTabHeaderId);
        activeMessageCountTabHeader.TextContent.Should().Be(QueueDetailPageLabel.ActiveTabHeader(0));

        var button = page.GetButton(QueueDetailPageLabel.RefreshButton);
        await button!.ClickAsync(new MouseEventArgs());

        activeMessageCountTabHeader = page.FindById(ActiveTabHeaderId);
        activeMessageCountTabHeader.TextContent.Should().Be(QueueDetailPageLabel.ActiveTabHeader(1));
    }

    [Fact(DisplayName = "Overview Detail Page should on refesh update the counter on dead-letter tab")]
    public async Task QueueDetailPageOnLoadShouldHaveOneMessageCountInDeadLetterQueueCount()
    {
        Application.Setup(this, out var clientName);

        var queueTestContext = Services.GetRequiredService<QueueTestContext>();
        await queueTestContext.ClearAll(ServicebusEntityNames.QueueDetailRefreshNoSession);

        var page = RenderComponent<QueueDetailPage>(p => p.Add(p => p.QueueName, ServicebusEntityNames.QueueDetailRefreshNoSession));
        await queueTestContext.SendAsync(ServicebusEntityNames.QueueDetailRefreshNoSession, new ServiceBusMessage("test message"));
        await queueTestContext.DeadLetterAsync(ServicebusEntityNames.QueueDetailRefreshNoSession, "Test reason");

        var deadLetterMessageCountTabHeader = page.FindById(DeadLetterTabHeaderId);
        deadLetterMessageCountTabHeader.TextContent.Should().Be(QueueDetailPageLabel.ActiveDeadLetterTabHeader(0));

        var button = page.GetButton(QueueDetailPageLabel.RefreshButton);
        await button!.ClickAsync(new MouseEventArgs());

        deadLetterMessageCountTabHeader = page.FindById(DeadLetterTabHeaderId);
        deadLetterMessageCountTabHeader.TextContent.Should().Be(QueueDetailPageLabel.ActiveDeadLetterTabHeader(1));
    }
}
