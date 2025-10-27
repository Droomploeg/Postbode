using Droomploeg.DreamOps.Application.Common;

namespace Droomploeg.DreamOps.Application.ServiceBus.Commands.Queues;

/// <summary>
/// Delete all messages command.
/// </summary>
/// <param name="QueueName">Name of the queue</param>
public record DeleteAllDeadLetterMessagesCommand(string QueueName) : ICommand;
