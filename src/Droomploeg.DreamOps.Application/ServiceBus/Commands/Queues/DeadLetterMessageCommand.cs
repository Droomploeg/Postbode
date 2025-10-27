using Droomploeg.DreamOps.Application.Common;

namespace Droomploeg.DreamOps.Application.ServiceBus.Commands.Queues;

public record DeadLetterMessageCommand<TReceiveMessage>(
    string QueueName,
    TReceiveMessage ReceiveMessage,
    string Source,
    string Reason,
    string Description) : ICommand
    where TReceiveMessage : notnull;
