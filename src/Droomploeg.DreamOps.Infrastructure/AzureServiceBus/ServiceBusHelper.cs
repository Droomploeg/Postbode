namespace Droomploeg.DreamOps.Infrastructure.AzureServiceBus;

public static class ServiceBusHelper
{
    public static Dictionary<string, object> GetDeadLetterProperties(string source, string reason, string description)
    {
        return new Dictionary<string, object>
        {
            { ServiceBusConstants.DeadLetterSourceKey, source },
            { ServiceBusConstants.DeadLetterReasonKey, reason },
            { ServiceBusConstants.DeadLetterErrorDescriptionKey, description }
        };
    }
    public static string FormatDeadLetterPath(string name)
        => $"{name}/$DeadLetterQueue";

    public static string FormatDeadLetterPath(string topic, string subscription)
        => $"{topic}/Subscriptions/{subscription}/$DeadLetterQueue";
}
