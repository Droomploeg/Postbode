using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Droomploeg.DreamOps.Infrastructure.HostedServices.WorkerService;

public class WorkerHostedService(IWorkerService queueService, ILogger<WorkerHostedService> logger)
    : BackgroundService
{
    private readonly ILogger<WorkerHostedService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await queueService.ExecuteNextAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing work item.");
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queued Hosted Service is stopping.");

        await base.StopAsync(stoppingToken);
    }
}
