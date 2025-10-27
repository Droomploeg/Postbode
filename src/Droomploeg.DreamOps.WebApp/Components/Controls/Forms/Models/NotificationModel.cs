using Droomploeg.DreamOps.Domain.Workers.Types;

namespace Droomploeg.DreamOps.WebApp.Components.Controls.Forms.Models;

public record NotificationModel(Guid Id, string Entity, string Message, NotificationType Type, DateTimeOffset Timestamp);
