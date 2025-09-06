namespace Droomploeg.DreamOps.Core.Models;

public record EntityRuntimeInfo(bool HasMessages, long ActiveMessageCount, long DeadLetterMessageCount, long ScheduleMessageCount, DateTimeOffset UpdatedAt);
