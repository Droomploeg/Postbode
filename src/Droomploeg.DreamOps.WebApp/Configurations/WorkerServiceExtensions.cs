using System.Diagnostics.CodeAnalysis;
using Droomploeg.DreamOps.Application.Workers.Dispatcher;
using Droomploeg.DreamOps.Infrastructure.Workers.Dispatcher;
using Droomploeg.DreamOps.WebApp.HostedServices;

namespace Droomploeg.DreamOps.WebApp.Configurations;

[ExcludeFromCodeCoverage( Justification = "Worker service configuration extensions")]
public static class WorkerServiceExtensions
{
    internal static IServiceCollection AddWorkerHostedServices(this IServiceCollection services)
    {
        services.AddHostedService<WorkerHostedService>();
        services.AddTransient<IWorkerDispatcher, WorkerDispatcher>();

        return services;
    }
}

