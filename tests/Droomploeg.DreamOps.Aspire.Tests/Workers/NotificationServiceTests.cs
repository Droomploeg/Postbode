using Droomploeg.DreamOps.Domain.Workers.Models;
using Droomploeg.DreamOps.Domain.Workers.Types;
using Droomploeg.DreamOps.Infrastructure.Workers.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Droomploeg.DreamOps.Aspire.Tests.Workers;

public class NotificationServiceTests
{
    private readonly WorkerService _workerService = new(NullLogger<WorkerService>.Instance);
    private readonly NotificationService _notificationService;

    public NotificationServiceTests()
    {
        _notificationService = new NotificationService(_workerService);
    }

    private WorkerItem AddScheduledItem(string entity = "test-queue")
    {
        var item = new WorkerItem("test-user", Guid.NewGuid(), entity, "Test",
            _ => Task.CompletedTask);
        _workerService.Add(item);
        return item;
    }

    // ===== TryUpdatePopupNotifications =====

    [Fact]
    public void TryUpdatePopup_WithNoItems_Should_ReturnFalse()
    {
        var result = _notificationService.TryUpdatePopupNotifications(
            [], DateTimeOffset.UtcNow, TimeSpan.FromSeconds(-5),
            out var output);

        Assert.False(result);
        Assert.Empty(output);
    }

    [Fact]
    public void TryUpdatePopup_WithNewItem_Should_ReturnTrue()
    {
        var item = AddScheduledItem();

        var result = _notificationService.TryUpdatePopupNotifications(
            [], DateTimeOffset.UtcNow, TimeSpan.FromSeconds(-5),
            out var output);

        Assert.True(result);
        Assert.Single(output);
        Assert.Equal(item.Id, output[0].Id);
    }

    [Fact]
    public void TryUpdatePopup_WithExistingActiveNotification_Should_KeepIt()
    {
        var item = AddScheduledItem();
        var existingNotification = new Notification(
            item.Id, item.Entity, item.Description,
            item.CreatedTimestamp, item.UpdatedTimestamp, item.State);

        var result = _notificationService.TryUpdatePopupNotifications(
            [existingNotification], DateTimeOffset.UtcNow, TimeSpan.FromSeconds(-5),
            out var output);

        Assert.Single(output);
        Assert.Equal(item.Id, output[0].Id);
    }

    [Fact]
    public void TryUpdatePopup_WithExpiredNotification_Should_RemoveIt()
    {
        var item = AddScheduledItem();
        var expiredNotification = new Notification(
            item.Id, item.Entity, item.Description,
            item.CreatedTimestamp,
            DateTimeOffset.UtcNow.AddSeconds(-30),
            WorkItemState.Completed);

        var result = _notificationService.TryUpdatePopupNotifications(
            [expiredNotification], DateTimeOffset.UtcNow, TimeSpan.FromSeconds(-5),
            out var output);

        Assert.True(result);
    }

    // ===== UpdateNotifications =====

    [Fact]
    public void UpdateNotifications_WithNoItems_Should_ReturnEmpty()
    {
        var result = _notificationService.UpdateNotifications([], DateTimeOffset.UtcNow);

        Assert.Empty(result);
    }

    [Fact]
    public void UpdateNotifications_WithNewItem_Should_IncludeIt()
    {
        var item = AddScheduledItem();

        var result = _notificationService.UpdateNotifications([], DateTimeOffset.UtcNow);

        Assert.Single(result);
        Assert.Equal(item.Id, result[0].Id);
        Assert.Equal(item.Entity, result[0].Entity);
    }

    [Fact]
    public void UpdateNotifications_WithActiveNotification_Should_KeepIt()
    {
        var item = AddScheduledItem();
        var notification = new Notification(
            item.Id, item.Entity, item.Description,
            item.CreatedTimestamp, item.UpdatedTimestamp,
            WorkItemState.Started);

        var result = _notificationService.UpdateNotifications([notification], DateTimeOffset.UtcNow);

        Assert.Single(result);
    }

    [Fact]
    public void UpdateNotifications_CompletedAndExpired_Should_RemoveIt()
    {
        var expiredId = Guid.NewGuid();
        var expiredNotification = new Notification(
            expiredId, "old-queue", "Old job",
            DateTimeOffset.UtcNow.AddMinutes(-10),
            DateTimeOffset.UtcNow.AddSeconds(-10),
            WorkItemState.Completed);

        var result = _notificationService.UpdateNotifications(
            [expiredNotification], DateTimeOffset.UtcNow);

        Assert.DoesNotContain(result, n => n.Id == expiredId);
    }

    [Fact]
    public void UpdateNotifications_CompletedButRecent_Should_KeepIt()
    {
        var item = AddScheduledItem();
        var recentNotification = new Notification(
            item.Id, item.Entity, item.Description,
            item.CreatedTimestamp,
            DateTimeOffset.UtcNow.AddSeconds(-2),
            WorkItemState.Completed);

        var result = _notificationService.UpdateNotifications(
            [recentNotification], DateTimeOffset.UtcNow);

        Assert.Single(result);
    }

    [Fact]
    public void UpdateNotifications_FailedAndExpired_Should_RemoveIt()
    {
        var failedId = Guid.NewGuid();
        var failedNotification = new Notification(
            failedId, "queue", "Failed job",
            DateTimeOffset.UtcNow.AddMinutes(-10),
            DateTimeOffset.UtcNow.AddSeconds(-10),
            WorkItemState.Failed);

        var result = _notificationService.UpdateNotifications(
            [failedNotification], DateTimeOffset.UtcNow);

        Assert.DoesNotContain(result, n => n.Id == failedId);
    }

    [Fact]
    public void UpdateNotifications_CancelledAndExpired_Should_RemoveIt()
    {
        var cancelledId = Guid.NewGuid();
        var cancelledNotification = new Notification(
            cancelledId, "queue", "Cancelled job",
            DateTimeOffset.UtcNow.AddMinutes(-10),
            DateTimeOffset.UtcNow.AddSeconds(-10),
            WorkItemState.Cancelled);

        var result = _notificationService.UpdateNotifications(
            [cancelledNotification], DateTimeOffset.UtcNow);

        Assert.DoesNotContain(result, n => n.Id == cancelledId);
    }

    // ===== Notification.Type mapping =====

    [Theory]
    [InlineData(WorkItemState.Scheduled, NotificationType.Information)]
    [InlineData(WorkItemState.Started, NotificationType.Information)]
    [InlineData(WorkItemState.Completed, NotificationType.Completed)]
    [InlineData(WorkItemState.Failed, NotificationType.Failure)]
    [InlineData(WorkItemState.Cancelled, NotificationType.Warning)]
    public void Notification_Type_Should_MapCorrectly(WorkItemState state, NotificationType expectedType)
    {
        var notification = new Notification(
            Guid.NewGuid(), "entity", "message",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, state);

        Assert.Equal(expectedType, notification.Type);
    }
}
