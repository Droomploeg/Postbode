using Droomploeg.DreamOps.Application.Common;
using Droomploeg.DreamOps.Application.ServiceBus.Adapters;
using Droomploeg.DreamOps.Application.ServiceBus.Commands.Queues;
using Droomploeg.DreamOps.Application.Workers.Services;
using Droomploeg.DreamOps.Domain.Workers.Models;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Handlers.ManagedIdentities.Queues;

public class ResubmitAllMessagesCommandHandler<TSendMessage, TReceiveMessage> : ICommandHandler<ResubmitAllMessagesCommand>
    where TReceiveMessage : notnull
{
    private readonly IDeadLetterQueueAdapter<TSendMessage, TReceiveMessage> _adapter;
    private readonly IWorkerService _service;

    public ResubmitAllMessagesCommandHandler(
        IDeadLetterQueueAdapter<TSendMessage, TReceiveMessage> adapter,
        IWorkerService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
    }

    public Task HandleAsync(ResubmitAllMessagesCommand command, CancellationToken cancellationToken = default)
    {
        var entityName = command.QueueName;
        var workItem = new WorkerItem(
            entityName,
            $"Resubmit all message to queue '{entityName}'",
            (token) => _adapter.ResubmitAllMessagesAsync(
                entityName, command.Options, token));

        _service.Add(workItem);
        return Task.CompletedTask;
    }
}
