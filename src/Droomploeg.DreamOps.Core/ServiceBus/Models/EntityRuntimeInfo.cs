namespace Droomploeg.DreamOps.Domain.ServiceBus.Models;

/// <summary>
/// Entity runtime information record.
/// </summary>
/// <param name="HasMessages"><see langword="true"/> if entity has messages</param>
/// <param name="TransferMessagesCount">Number of transfer messages</param>
/// <param name="ActiveMessageCount">Number of active messages</param>
/// <param name="TransferDeadLetterMessageCount">Number of transfer dead letter message</param>
/// <param name="DeadLetterMessageCount">Number of dead letter messages</param>
/// <param name="ScheduleMessageCount">Number of scheduled messages</param>
/// <param name="TotalMessageCount">Total number of messages</param>
/// <param name="UpdatedAt"><see cref="DateTimeOffset"/> last updated</param>
public record EntityRuntimeInfo(bool HasMessages, long TransferMessagesCount, long ActiveMessageCount, long TransferDeadLetterMessageCount, long DeadLetterMessageCount, long ScheduleMessageCount, long TotalMessageCount, DateTimeOffset UpdatedAt);
