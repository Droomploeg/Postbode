using System.Diagnostics.CodeAnalysis;
using Droomploeg.Postbode.Domain.ServiceBus.Types;

namespace Droomploeg.Postbode.Infrastructure.Contexts;

/// <summary>
/// Application context.
/// </summary>
[ExcludeFromCodeCoverage( Justification = "This class is responsible for holding contextual information about the application, such as correlation identifiers and user information. Testing this class would not provide significant value in terms of code coverage, as it primarily serves as a data container.")]
public class ApplicationContext
{
    /// <summary>
    /// Correlation identifier.
    /// </summary>
    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User name.
    /// </summary>
    public string UserName { get; set; } = "Anonymous";

    /// <summary>
    /// Current Service Bus connection.
    /// </summary>
    public ServiceBusConnection CurrentConnection { get; set; } = ServiceBusConnection.Undefined;
}
