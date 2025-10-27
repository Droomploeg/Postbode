using Droomploeg.DreamOps.WebApp.HostedServices;

namespace Droomploeg.DreamOps.WebApp.Configurations;

public static class WorkerServiceExtenstions
{
    internal static IServiceCollection AddWorkerHostedServices(this IServiceCollection services)
    {
        services.AddHostedService<WorkerHostedService>();
        return services;
    }
}

