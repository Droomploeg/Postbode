using Droomploeg.DreamOps.Application.Common;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Application.ServiceBus.Commands.Queues;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Handlers.OnBehalfOf.Queues;

public class SendMessageCommandHandler<TSendMessage, TReceiveMessage> : ICommandHandler<SendMessageCommand<TSendMessage>>
    where TReceiveMessage : notnull
{
    private readonly IActiveQueueAdapter<TSendMessage, TReceiveMessage> _adapter;

    public SendMessageCommandHandler(IActiveQueueAdapter<TSendMessage, TReceiveMessage> adapter)
    {
        _adapter = adapter;
    }

    public Task HandleAsync(SendMessageCommand<TSendMessage> command, CancellationToken cancellationToken = default)
    {
        return _adapter.SendAsync(command.QueueName, [command.Message], cancellationToken);
    }
}
