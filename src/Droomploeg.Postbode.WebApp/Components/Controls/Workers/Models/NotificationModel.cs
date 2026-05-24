using Droomploeg.Postbode.Domain.Workers.Types;

namespace Droomploeg.Postbode.WebApp.Components.Controls.Workers.Models;

public record NotificationModel(Guid Id, string Entity, string Message, string State, NotificationType Type, DateTimeOffset TimestampCreated, DateTimeOffset TimestampStateChange);
