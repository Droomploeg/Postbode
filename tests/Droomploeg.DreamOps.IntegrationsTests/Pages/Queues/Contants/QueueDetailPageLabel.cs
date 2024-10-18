namespace Droomploeg.DreamOps.IntegrationsTests.Pages.Queues.Contants;

internal static class QueueDetailPageLabel
{
    internal const string RefreshButton = "Refresh";
    internal const string NewMessageButton = "NewMessage";
    internal const string SendButton = "Send";

    internal static string ActiveTabHeader(int messages)
        => $"Active ({messages})";

    internal static string ActiveDeadLetterTabHeader(int messages)
        => $"Dead-letter ({messages})";

}
