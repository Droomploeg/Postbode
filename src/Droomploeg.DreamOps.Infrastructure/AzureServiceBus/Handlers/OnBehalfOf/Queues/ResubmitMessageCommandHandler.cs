using Droomploeg.DreamOps.Application.Common;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Application.ServiceBus.Commands.Queues;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Handlers.OnBehalfOf.Queues;

public class ResubmitMessageCommandHandler<TSendMessage, TReceiveMessage> : ICommandHandler<ResubmitMessageCommand<TSendMessage, TReceiveMessage>>
    where TReceiveMessage : notnull
{
    private readonly IDeadLetterQueueAdapter<TSendMessage, TReceiveMessage> _adapter;

    public ResubmitMessageCommandHandler(IDeadLetterQueueAdapter<TSendMessage, TReceiveMessage> adapter)
    {
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
    }

    public Task HandleAsync(ResubmitMessageCommand<TSendMessage, TReceiveMessage> command, CancellationToken cancellationToken = default)
    {
        return _adapter.ResubmitMessageAsync(
                command.QueueName,
                command.ReceiveMessage,
                command.RepairMessage,
                command.ResubmitOptions,
                cancellationToken);
    }
}

