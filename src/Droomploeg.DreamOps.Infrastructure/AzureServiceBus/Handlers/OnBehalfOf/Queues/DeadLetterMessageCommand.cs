using Droomploeg.DreamOps.Application.Common;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Application.ServiceBus.Commands.Queues;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Handlers.OnBehalfOf.Queues;

public class DeadLetterMessageCommand<TSendMessage, TReceiveMessage> : ICommandHandler<DeadLetterMessageCommand<TReceiveMessage>>
    where TReceiveMessage : notnull
{
    private readonly IActiveQueueAdapter<TSendMessage, TReceiveMessage> _adapter;

    public DeadLetterMessageCommand(IActiveQueueAdapter<TSendMessage, TReceiveMessage> adapter)
    {
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
    }

    public Task HandleAsync(DeadLetterMessageCommand<TReceiveMessage> command, CancellationToken cancellationToken = default)
    {
        return _adapter.DeadLetterMessagesAsync(command.QueueName, 
            command.ReceiveMessage, 
            command.Source, 
            command.Reason, 
            command.Description, 
            cancellationToken);
    }
}
