using Droomploeg.DreamOps.Domain.Workers.Types;

namespace Droomploeg.DreamOps.WebApp.Components.Controls.Workers.Models;

public record NotificationModel(Guid Id, string Entity, string Message, NotificationType Type, DateTimeOffset Timestamp);
