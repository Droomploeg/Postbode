using Droomploeg.DreamOps.Domain.Workers.Types;

namespace Droomploeg.DreamOps.WebApp.Components.Controls.Workers.Models;

public record NotificationModel(Guid Id, string Entity, string Message, string State, NotificationType Type, DateTimeOffset TimestampCreated, DateTimeOffset TimestampStateChange);
