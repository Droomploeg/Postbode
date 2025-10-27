using Droomploeg.DreamOps.Application.Common;
using Droomploeg.DreamOps.Domain.ServiceBus.Models;

namespace Droomploeg.DreamOps.Application.ServiceBus.Commands.Queues;

public record ResubmitMessageCommand<TSendMessage, TReceiveMessage>(
        string QueueName,
        TReceiveMessage ReceiveMessage,
        TSendMessage RepairMessage,
        ResubmitOptions ResubmitOptions) : ICommand
    where TReceiveMessage : notnull;
