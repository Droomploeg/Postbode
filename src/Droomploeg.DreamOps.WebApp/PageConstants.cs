namespace Droomploeg.DreamOps.WebApp;

internal class PageConstants
{
    internal const string LoginPath = $"{AuthenticationGroup}{LoginPage}";
    internal const string LogoutPath = $"{AuthenticationGroup}{LogoutPage}";

    internal const string AuthenticationGroup = "/authentication";
    internal const string LoginPage = "/login";
    internal const string LogoutPage = "/logout";

    internal const string HomePage = "/";
    internal const string OverviewPage = "/overview";

    internal const string QueueOverviewPage = "/queue-overview";
    internal const string QueueDetailPage = "/queue";
    internal const string QueueDetailPageWithParameters = "/queue/{queueName}";

    internal const string SubscriptionOverviewPage = "/subscription-overview";
    internal const string SubscriptionDetailPage = "/subscription";
    internal const string SubscriptionDetailPageWithParameters = "/subscription/{topicName}/{subscriptionName}";
}
