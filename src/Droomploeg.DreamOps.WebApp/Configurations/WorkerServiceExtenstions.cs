using Microsoft.AspNetCore.ResponseCompression;
using Droomploeg.DreamOps.Infrastructure.HostedServices.WorkerService;
using Droomploeg.DreamOps.WebApp.HostedServices;

namespace Droomploeg.DreamOps.WebApp.Configurations;

public static class WorkerServiceExtenstions
{
    internal static IServiceCollection AddWorkerHostedServices(this IServiceCollection services)
    {
        services.AddHostedService<WorkerHostedService>();
        services.AddSingleton<IWorkerService, WorkerService>();
        services.AddSingleton<IWorkerMonitor, WorkerMonitor>();

        return services;
    }
}

