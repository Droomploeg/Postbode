using System.Diagnostics.CodeAnalysis;
using Droomploeg.Postbode.Domain.ServiceBus.Types;

namespace Droomploeg.Postbode.Infrastructure.AzureServiceBus.Mappers;

/// <summary>
/// Map message counts to a domain <see cref="EntityHealthState"/> value.
/// </summary>
[ExcludeFromCodeCoverage( Justification = "Mapper class")]
internal static class EntityHealthStateMapper
{
    /// <summary>
    /// Map message counts values to the most relevant <see cref="EntityHealthState"/>.
    /// </summary>
    /// <param name="numberOfActiveMessage">Number of active messages.</param>
    /// <param name="numberOfScheduledMessages">Number of scheduled messages.</param>
    /// <param name="numberOfTransferMessages">Number of transfer messages.</param>
    /// <param name="numberOfDeadLetterMessages">Number of dead-letter messages.</param>
    /// <returns>The <see cref="EntityHealthState"/> representing the highest-priority state.</returns>
    internal static EntityHealthState Map(
        long numberOfActiveMessage,
        long numberOfScheduledMessages,
        long numberOfTransferMessages,
        long numberOfDeadLetterMessages)
    {
        if (numberOfDeadLetterMessages > 0)
        {
            return EntityHealthState.HasDeadLetterMessages;
        }
        if (numberOfScheduledMessages > 0)
        {
            return EntityHealthState.HasScheduledMessages;
        }
        if (numberOfTransferMessages > 0)
        {
            return EntityHealthState.TransferMessageCount;
        }
        if (numberOfActiveMessage > 0)
        {
            return EntityHealthState.HasActiveMessages;
        }

        return EntityHealthState.NoMessages;
    }
}
