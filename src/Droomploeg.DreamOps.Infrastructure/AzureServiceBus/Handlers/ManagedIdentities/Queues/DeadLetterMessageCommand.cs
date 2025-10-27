using Droomploeg.DreamOps.Application.Common;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Application.ServiceBus.Commands.Queues;
using Droomploeg.DreamOps.Application.Workers.Services;
using Droomploeg.DreamOps.Domain.Workers.Models;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Handlers.ManagedIdentities.Queues;

public class DeadLetterMessageCommand<TSendMessage, TReceiveMessage> : ICommandHandler<DeadLetterMessageCommand<TReceiveMessage>>
    where TReceiveMessage : notnull
{
    private readonly IActiveQueueAdapter<TSendMessage, TReceiveMessage> _adapter;
    private readonly IWorkerService _service;

    public DeadLetterMessageCommand(
        IActiveQueueAdapter<TSendMessage, TReceiveMessage> adapter,
        IWorkerService service)
    {
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public Task HandleAsync(DeadLetterMessageCommand<TReceiveMessage> command, CancellationToken cancellationToken = default)
    {
        var entityName = command.QueueName;
        var workItem = new WorkerItem(
            entityName,
            $"Deadletter message from queue '{entityName}' to dead-letter",
            (token) => _adapter.DeadLetterMessagesAsync(
                entityName, command.ReceiveMessage, command.Source, command.Reason, command.Description, token));

        _service.Add(workItem);
        return Task.CompletedTask;
    }
}
