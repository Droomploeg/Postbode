using Droomploeg.DreamOps.Application.Common;

namespace Droomploeg.DreamOps.Application.ServiceBus.Commands.Queues;

public record SendMessageCommand<TSendMessage>(string QueueName, TSendMessage Message) : ICommand;
