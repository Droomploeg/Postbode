namespace Droomploeg.DreamOps.Domain.ServiceBus.Models;

public record EntityRuntimeInfo(bool HasMessages, long TransferMessagesCount, long ActiveMessageCount, long TransferDeadLetterMessageCount, long DeadLetterMessageCount, long ScheduleMessageCount, long TotalMessageCount, DateTimeOffset UpdatedAt);
