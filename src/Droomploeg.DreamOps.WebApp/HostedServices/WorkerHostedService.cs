using Droomploeg.DreamOps.Application.Workers.Services;

namespace Droomploeg.DreamOps.WebApp.HostedServices;

/// <summary>
/// Work items background service.
/// </summary>
/// <param name="workerService"><see cref="IWorkerService"/></param>
/// <param name="logger"><see cref="ILogger"/></param>
public class WorkerHostedService(IWorkerService workerService, ILogger<WorkerHostedService> logger)
    : BackgroundService
{
    private readonly IWorkerService _workerService = workerService;
    private readonly ILogger<WorkerHostedService> _logger = logger;

    /// <inheritdoc cref="BackgroundService" />
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _workerService.ExecuteNextAsync(cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            catch (Exception ex)
            {
                // monitor service update to failed
                _logger.LogError(ex, "Error occurred executing work item.");
            }
        }
    }

    /// <inheritdoc cref="BackgroundService" />
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Queued Hosted Service is stopping.");

        await base.StopAsync(cancellationToken);
    }
}
