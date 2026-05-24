using System.Diagnostics.CodeAnalysis;
using Droomploeg.Postbode.Application.Workers.Dispatcher;
using Droomploeg.Postbode.Infrastructure.Workers.Dispatcher;
using Droomploeg.Postbode.WebApp.HostedServices;

namespace Droomploeg.Postbode.WebApp.Configurations;

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

