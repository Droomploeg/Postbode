using Droomploeg.Postbode.Application.Workers.Services;

namespace Droomploeg.Postbode.WebApp.HostedServices;

/// <summary>
/// Work items background service.
/// </summary>
/// <param name="workerService"><see cref="IWorkerService"/></param>
/// <param name="logger"><see cref="ILogger"/></param>
public class WorkerHostedService(IWorkerService workerService, ILogger<WorkerHostedService> logger)
    : BackgroundService
{
    /// <inheritdoc cref="BackgroundService" />
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await workerService.ExecuteNextAsync(cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            catch (Exception ex)
            {
                // monitor service update to fail
                logger.LogError(ex, "Error occurred executing work item.");
            }
        }
    }

    /// <inheritdoc cref="BackgroundService" />
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Queued Hosted Service is stopping.");

        await base.StopAsync(cancellationToken);
    }
}
