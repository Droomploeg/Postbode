using Azure.Messaging.ServiceBus;

namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus;

public static class ServiceBusConstants
{
    public const string ClientName = "ServiceBusConnection";
    public const string AdminClientName = "ServiceBusConnection";
    public const string ResubmitKey = "ResubmitKey";

    public const string DeadLetterSourceKey = "DeadLetterSource";
    public const string DeadLetterReasonKey = "DeadLetterReason";
    public const string DeadLetterErrorDescriptionKey = "DeadLetterErrorDescription";

    internal const int BucketSize = 250;

    internal static readonly ServiceBusReceiverOptions PeekLockOptions = new() { ReceiveMode = ServiceBusReceiveMode.PeekLock };
    internal static readonly ServiceBusSessionReceiverOptions PeekLockSessionOptions = new() { ReceiveMode = ServiceBusReceiveMode.PeekLock };
    internal static readonly string[] DeadletterReasonKeys = [DeadLetterSourceKey, DeadLetterReasonKey, DeadLetterErrorDescriptionKey];

}
