using Droomploeg.Postbode.Application.ServiceBus.Factories;
using Droomploeg.Postbode.Infrastructure.AzureServiceBus.Exceptions;
using Droomploeg.Postbode.Infrastructure.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;

namespace Droomploeg.Postbode.Infrastructure.AzureServiceBus.Factories;

/// <summary>
/// Adapter factory implementation.
/// </summary>
/// <typeparam name="T">Adapter</typeparam>
public partial class AdapterFactory<T> : IAdapterFactory<T> where T : notnull
{
    private readonly ApplicationContext _context;
    private readonly IServiceProvider _provider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AdapterFactory<T>> _logger;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="provider"><see cref="IServiceProvider"/> of the current scope.</param>
    /// <param name="scopeFactory">Creates the scope of a background job. Unlike the current scope,
    /// it is still usable after the circuit has ended.</param>
    /// <param name="context"><see cref="ApplicationContext"/></param>
    /// <param name="logger">Logger for this factory.</param>
    /// <exception cref="ArgumentNullException">When one of the arguments is null</exception>
    public AdapterFactory(
        IServiceProvider provider,
        IServiceScopeFactory scopeFactory,
        ApplicationContext context,
        ILogger<AdapterFactory<T>> logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc cref="IAdapterFactory{T}.Create"/>
    public T Create()
        => _provider.GetRequiredService<T>();

    /// <inheritdoc cref="IAdapterFactory{T}.CreateBackgroundAction"/>
    public Func<CancellationToken, Task> CreateBackgroundAction(Func<T, CancellationToken, Task> work)
    {
        ArgumentNullException.ThrowIfNull(work);

        // Capture the values now: the scoped context may be gone by the time the job runs.
        var correlationId = _context.CorrelationId;
        var connection = _context.CurrentConnection;
        var userName = _context.UserName;
        var user = _context.User;

        return async cancellationToken =>
        {
            LogBackgroundScopeStarting(correlationId);

            await using var scope = _scopeFactory.CreateAsyncScope();
            var scopedContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            scopedContext.CorrelationId = correlationId;
            scopedContext.CurrentConnection = connection;
            scopedContext.UserName = userName;
            scopedContext.User = user;

            var adapter = scope.ServiceProvider.GetRequiredService<T>();
            try
            {
                await work(adapter, cancellationToken);
            }
            catch (Exception exception) when (IsSignInRequired(exception))
            {
                LogSignInRequired(correlationId);
                throw new UserSignInRequiredException(exception);
            }
        };
    }

    private static bool IsSignInRequired(Exception exception)
    {
        // The Service Bus SDK may wrap the exception of the credential.
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is MicrosoftIdentityWebChallengeUserException or Microsoft.Identity.Client.MsalUiRequiredException)
            {
                return true;
            }
        }

        return false;
    }

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Starting background job scope for correlation {CorrelationId}")]
    private partial void LogBackgroundScopeStarting(Guid correlationId);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Background job for correlation {CorrelationId} stopped: the user has to sign in again")]
    private partial void LogSignInRequired(Guid correlationId);
}
