using Droomploeg.DreamOps.Infrastructure.AzureServiceBus;
using Droomploeg.DreamOps.Infrastructure.HostedServices.WorkerService;
using Droomploeg.DreamOps.WebApp.Configurations.Options;

namespace Droomploeg.DreamOps.WebApp.HostedServices;

/// <summary>
/// Work items background service.
/// </summary>
/// <param name="workerService"><see cref="IWorkerService"/></param>
/// <param name="logger"><see cref="ILogger"/></param>
public class WorkerHostedService(
    IWorkerService workerService, 
    IConfiguration configuration,
    //IServiceBusConnectionAccessor connectionAccessor,
    ILogger<WorkerHostedService> logger)
    : BackgroundService
{
    private readonly ILogger<WorkerHostedService> _logger = logger;

    /// <inheritdoc cref="BackgroundService" />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connectionList = configuration.GetSection(AzureServiceBusConnection.SectionName).Get<List<AzureServiceBusConnection>>() ?? [];
        if (!connectionList.Any())
        {
            _logger.LogInformation("WorkerHostedService not started, no servicebus connections found with background service enabled");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await workerService.ExecuteNextAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);

                //var connection = await connectionAccessor.GetCurrentAsync();
                //Console.WriteLine(connection.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing work item.");
            }
        }
    }

    /// <inheritdoc cref="BackgroundService" />
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queued Hosted Service is stopping.");

        await base.StopAsync(stoppingToken);
    }
}
