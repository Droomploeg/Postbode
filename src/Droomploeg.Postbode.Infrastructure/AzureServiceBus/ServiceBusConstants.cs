using Azure.Messaging.ServiceBus;

namespace Droomploeg.Postbode.Infrastructure.AzureServiceBus;

/// <summary>
/// Constants for Azure Service Bus configuration.
/// </summary>
public static class ServiceBusConstants
{
    /// <summary>Client name used for Service Bus client registration.</summary>
    public const string ClientName = "ServiceBusConnection";

    /// <summary>Client name used for Service Bus administration client registration.</summary>
    public const string AdminClientName = "ServiceBusConnection";

    /// <summary>Application property key used to track resubmitted messages.</summary>
    public const string ResubmitKey = "ResubmitKey";

    /// <summary>Application property key for the dead-letter source.</summary>
    public const string DeadLetterSourceKey = "DeadLetterSource";

    /// <summary>Application property key for the dead-letter reason.</summary>
    public const string DeadLetterReasonKey = "DeadLetterReason";

    /// <summary>Application property key for the dead-letter error description.</summary>
    public const string DeadLetterErrorDescriptionKey = "DeadLetterErrorDescription";

    /// <summary>Maximum number of messages to process per batch.</summary>
    internal const int BucketSize = 250;

    /// <summary>Default receiver options configured for peek-lock mode.</summary>
    internal static ServiceBusReceiverOptions PeekLockOptions => new() { ReceiveMode = ServiceBusReceiveMode.PeekLock };

    /// <summary>Default session receiver options configured for peek-lock mode.</summary>
    internal static ServiceBusSessionReceiverOptions PeekLockSessionOptions => new() { ReceiveMode = ServiceBusReceiveMode.PeekLock };
}
