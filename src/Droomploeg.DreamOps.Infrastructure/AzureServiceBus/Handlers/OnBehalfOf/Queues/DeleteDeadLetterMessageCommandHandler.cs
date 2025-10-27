using Droomploeg.DreamOps.Application.Common;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Application.ServiceBus.Commands.Queues;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Handlers.OnBehalfOf.Queues;

public class DeleteDeadLetterMessageCommandHandler<TSendMessage, TReceiveMessage> : ICommandHandler<DeleteDeadLetterMessageCommand<TReceiveMessage>>
    where TReceiveMessage : notnull
{
    private readonly IDeadLetterQueueAdapter<TSendMessage, TReceiveMessage> _adapter;

    public DeleteDeadLetterMessageCommandHandler(
        IDeadLetterQueueAdapter<TSendMessage, TReceiveMessage> adapter)
    {
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
    }

    public Task HandleAsync(DeleteDeadLetterMessageCommand<TReceiveMessage> command, CancellationToken cancellationToken = default)
    {
        return _adapter.DeleteMessageAsync(
                command.QueueName, command.Message, cancellationToken);
    }
}
