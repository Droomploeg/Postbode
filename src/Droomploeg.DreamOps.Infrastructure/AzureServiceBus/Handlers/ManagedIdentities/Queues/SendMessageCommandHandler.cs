using Droomploeg.DreamOps.Application.Common;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Application.ServiceBus.Commands.Queues;
using Droomploeg.DreamOps.Application.Workers.Services;
using Droomploeg.DreamOps.Domain.Workers.Models;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Handlers.ManagedIdentities.Queues;

public class SendMessageCommandHandler<TSendMessage, TReceiveMessage> : ICommandHandler<SendMessageCommand<TSendMessage>>
    where TReceiveMessage : notnull
{
    private readonly IActiveQueueAdapter<TSendMessage, TReceiveMessage> _adapter;
    private readonly IWorkerService _service;

    public SendMessageCommandHandler(
        IActiveQueueAdapter<TSendMessage, TReceiveMessage> adapter,
        IWorkerService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
    }

    public Task HandleAsync(SendMessageCommand<TSendMessage> command, CancellationToken cancellationToken = default)
    {
        var entityName = command.QueueName;
        var workItem = new WorkerItem(
            entityName,
            $"Send message to queue '{entityName}'",
            (token) => _adapter.SendAsync(entityName, [command.Message], token));

        _service.Add(workItem);
        return Task.CompletedTask;
    }
}
