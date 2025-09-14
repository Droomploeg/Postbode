using Azure.Messaging.ServiceBus;
using Bunit;
using Droomploeg.DreamOps.IntegrationsTests.Common.BUnit;
using Droomploeg.DreamOps.IntegrationsTests.Common.Controls;
using Droomploeg.DreamOps.IntegrationsTests.Common.ServiceBus;
using Droomploeg.DreamOps.IntegrationsTests.Pages.Queues.Contants;
using Droomploeg.DreamOps.IntegrationsTests.Pages.Queues.Extensions;
using Droomploeg.DreamOps.IntegrationsTests.Pages.Queues.GridColumns;
using Droomploeg.DreamOps.WebApp.Components.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Droomploeg.DreamOps.IntegrationsTests.Pages.Queues;

[CollectionDefinition(ServicebusEntityNames.QueueOverviewRefresh, DisableParallelization = true)]
public class QueueOverviewPageTests : TestContext
{
    [Fact(DisplayName = "Overview Page without messages should have zero message count")]
    public async Task OverviewPagWithoutMessagesShouldHaveZerosOnMessageCount()
    {
        var connection = await Application.SetupAsync(this);

        var queueTestContext = Services.GetRequiredService<QueueTestContext>();
        await queueTestContext.ClearAll(ServicebusEntityNames.QueueOverviewRefresh);

        var page = RenderComponent<QueueOverviewPage>();
        var grid = page.GetGrid();

        grid.WaitForState(() => grid.Instance.IsLoading() == false, TimeSpan.FromSeconds(5));

        page.GetColumnValueOrRow(ServicebusEntityNames.QueueOverviewRefresh, QueueOverviewGridColumn.ActiveMessageCount).Should().Be(0);
        page.GetColumnValueOrRow(ServicebusEntityNames.QueueOverviewRefresh, QueueOverviewGridColumn.ScheduledMessageCount).Should().Be(0);
        page.GetColumnValueOrRow(ServicebusEntityNames.QueueOverviewRefresh, QueueOverviewGridColumn.DeadLetterCount).Should().Be(0);
    }

    [Fact(DisplayName = "Overview Page with 1 active messages should have 1 active message")]
    public async Task OverviewPageWithOneActiveMessageShouldShowOneActiveMessage()
    {
        var connection = await Application.SetupAsync(this);

        var queueTestContext = this.Services.GetRequiredService<QueueTestContext>();
        var serviceBusMessage = new ServiceBusMessage("test message")
        {
            Subject = "Test message"
        };

        await queueTestContext.ClearAll(ServicebusEntityNames.QueueOverviewRefresh);
        await queueTestContext.SendAsync(ServicebusEntityNames.QueueOverviewRefresh, serviceBusMessage);

        var page = RenderComponent<QueueOverviewPage>();
        var grid = page.GetGrid();

        grid.WaitForState(() => grid.Instance.IsLoading() == false, TimeSpan.FromSeconds(5));

        page.GetColumnValueOrRow(ServicebusEntityNames.QueueOverviewRefresh, QueueOverviewGridColumn.ActiveMessageCount).Should().Be(1);
        page.GetColumnValueOrRow(ServicebusEntityNames.QueueOverviewRefresh, QueueOverviewGridColumn.ScheduledMessageCount).Should().Be(0);
        page.GetColumnValueOrRow(ServicebusEntityNames.QueueOverviewRefresh, QueueOverviewGridColumn.DeadLetterCount).Should().Be(0);
    }

    [Fact(DisplayName = "Overview Page with 1 dead-letter messages should have 1 dead-letter message")]
    public async Task OverviewPageWithOneDeadletterMessageShouldShowOneDeadLetterMessage()
    {
        var connection = await Application.SetupAsync(this);

        var queueTestContext = Services.GetRequiredService<QueueTestContext>();
        var serviceBusMessage = new ServiceBusMessage("test message")
        {
            Subject = "Test message"
        };

        await queueTestContext.ClearAll(ServicebusEntityNames.QueueOverviewRefresh);
        await queueTestContext.SendAsync(ServicebusEntityNames.QueueOverviewRefresh, serviceBusMessage);
        await queueTestContext.DeadLetterAsync(ServicebusEntityNames.QueueOverviewRefresh, "Test reason");

        var page = RenderComponent<QueueOverviewPage>();
        var grid = page.GetGrid();

        grid.WaitForState(() => grid.Instance.IsLoading() == false, TimeSpan.FromSeconds(5));

        page.GetColumnValueOrRow(ServicebusEntityNames.QueueOverviewRefresh, QueueOverviewGridColumn.ActiveMessageCount).Should().Be(0);
        page.GetColumnValueOrRow(ServicebusEntityNames.QueueOverviewRefresh, QueueOverviewGridColumn.ScheduledMessageCount).Should().Be(0);
        page.GetColumnValueOrRow(ServicebusEntityNames.QueueOverviewRefresh, QueueOverviewGridColumn.DeadLetterCount).Should().Be(1);
    }

    [Fact(DisplayName = "Overview Page after refresh should update queues and counters")]
    public async Task OverviewPageOnActionRefreshShouldRefreshPageAndCounters()
    {
        var connection = await Application.SetupAsync(this);

        var queueTestContext = Services.GetRequiredService<QueueTestContext>();
        await queueTestContext.ClearAll(ServicebusEntityNames.QueueOverviewRefresh);

        var page = RenderComponent<QueueOverviewPage>();
        var grid = page.GetGrid();

        grid.WaitForState(() => grid.Instance.IsLoading() == false, TimeSpan.FromSeconds(5));

        page.GetColumnValueOrRow(ServicebusEntityNames.QueueOverviewRefresh, QueueOverviewGridColumn.ActiveMessageCount).Should().Be(0);
        page.GetColumnValueOrRow(ServicebusEntityNames.QueueOverviewRefresh, QueueOverviewGridColumn.ScheduledMessageCount).Should().Be(0);
        page.GetColumnValueOrRow(ServicebusEntityNames.QueueOverviewRefresh, QueueOverviewGridColumn.DeadLetterCount).Should().Be(0);

        var serviceBusMessage = new ServiceBusMessage("test message")
        {
            Subject = "Test message"
        };
        await queueTestContext.SendAsync(ServicebusEntityNames.QueueOverviewRefresh, serviceBusMessage);

        var button = page.GetButton(QueueOverviewPageLabel.RefreshButton);
        await button!.ClickAsync(new MouseEventArgs());

        grid.WaitForState(() => grid.Instance.IsLoading() == false, TimeSpan.FromSeconds(5));

        page.GetColumnValueOrRow(ServicebusEntityNames.QueueOverviewRefresh, QueueOverviewGridColumn.ActiveMessageCount).Should().Be(1);
        page.GetColumnValueOrRow(ServicebusEntityNames.QueueOverviewRefresh, QueueOverviewGridColumn.ScheduledMessageCount).Should().Be(0);
        page.GetColumnValueOrRow(ServicebusEntityNames.QueueOverviewRefresh, QueueOverviewGridColumn.DeadLetterCount).Should().Be(0);
    }
}
