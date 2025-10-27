using Droomploeg.DreamOps.Application.Common;

namespace Droomploeg.DreamOps.Application.ServiceBus.Commands.Queues;

public record DeleteDeadLetterMessageCommand<TReceiveMessage>(
    string QueueName,
    TReceiveMessage Message) : ICommand
    where TReceiveMessage : notnull;
