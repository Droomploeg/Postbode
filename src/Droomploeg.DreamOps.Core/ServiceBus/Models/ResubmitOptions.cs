namespace Droomploeg.DreamOps.Domain.ServiceBus.Models;

/// <summary>
/// Resubmit options for messages.
/// </summary>
/// <param name="GenerateMessageIds"><see langword="true"/> when generate new message ids</param>
/// <param name="DeleteMessage"><see langword="true"/> when delete message after submit</param>
public record ResubmitOptions(bool GenerateMessageIds, bool DeleteMessage);
