namespace Droomploeg.DreamOps.Core.Models;

public record EntityRuntimeInfo(bool hasMessages, long ActiveMessageCount, long DeadLetterMessageCount, long ScheduleMessageCount, DateTimeOffset UpdatedAt);
