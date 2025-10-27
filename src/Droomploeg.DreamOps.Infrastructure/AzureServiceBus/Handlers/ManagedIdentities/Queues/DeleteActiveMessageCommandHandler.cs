using Droomploeg.DreamOps.Application.Common;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Application.ServiceBus.Commands.Queues;
using Droomploeg.DreamOps.Application.Workers.Services;
using Droomploeg.DreamOps.Domain.Workers.Models;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Handlers.ManagedIdentities.Queues;

public class DeleteActiveMessageCommandHandler<TSendMessage, TReceiveMessage> : ICommandHandler<DeleteActiveMessageCommand<TReceiveMessage>>
    where TReceiveMessage : notnull
{
    private readonly IActiveQueueAdapter<TSendMessage, TReceiveMessage> _adapter;
    private readonly IWorkerService _service;

    public DeleteActiveMessageCommandHandler(
        IActiveQueueAdapter<TSendMessage, TReceiveMessage> adapter,
        IWorkerService service)
    {
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public Task HandleAsync(DeleteActiveMessageCommand<TReceiveMessage> command, CancellationToken cancellationToken = default)
    {
        var entityName = command.QueueName;
        var workItem = new WorkerItem(
            entityName,
            $"Delete message from queue '{entityName}'",
            (token) => _adapter.DeleteMessageAsync(
                entityName, command.Message, token));

        _service.Add(workItem);
        return Task.CompletedTask;
    }
}
