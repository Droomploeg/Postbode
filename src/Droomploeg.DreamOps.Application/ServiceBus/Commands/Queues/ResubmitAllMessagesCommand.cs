using Droomploeg.DreamOps.Application.Common;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;

namespace Droomploeg.DreamOps.Application.ServiceBus.Commands.Queues;

/// <summary>
/// Resubmit all messages command.
/// </summary>
/// <param name="QueueName">Name of the queue</param>
/// <param name="Options"><see cref="ResubmitAllMessagesCommand"/></param>
public record ResubmitAllMessagesCommand(
    string QueueName,
    ResubmitOptions Options) : ICommand;
