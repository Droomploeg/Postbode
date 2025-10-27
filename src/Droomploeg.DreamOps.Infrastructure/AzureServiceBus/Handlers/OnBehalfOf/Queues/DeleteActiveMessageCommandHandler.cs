using Droomploeg.DreamOps.Application.Common;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Application.ServiceBus.Commands.Queues;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Handlers.OnBehalfOf.Queues;

public class DeleteActiveMessageCommandHandler<TSendMessage, TReceiveMessage> : ICommandHandler<DeleteActiveMessageCommand<TReceiveMessage>>
    where TReceiveMessage : notnull
{
    private readonly IActiveQueueAdapter<TSendMessage, TReceiveMessage> _adapter;

    public DeleteActiveMessageCommandHandler(IActiveQueueAdapter<TSendMessage, TReceiveMessage> adapter)
    {
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
    }

    public Task HandleAsync(DeleteActiveMessageCommand<TReceiveMessage> command, CancellationToken cancellationToken = default)
    {
        return _adapter.DeleteMessageAsync(command.QueueName, command.Message, cancellationToken);
    }
}
