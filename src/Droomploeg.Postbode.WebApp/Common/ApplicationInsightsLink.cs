using System.Web;

namespace Droomploeg.Postbode.WebApp.Common;

public class ApplicationInsightsLink
{
    private const string BaseUrl = "https://portal.azure.com";
    private const string SectionName = "ApplicationInsightLink";
    private const string AzureDirectoryNameKey = $"{SectionName}:AzureDirectoryName";
    private const string SubscriptionIdKey = $"{SectionName}:SubscriptionId";
    private const string ResourceGroupNameKey = $"{SectionName}:ResourceGroupName";
    private const string ResourceNameKey = $"{SectionName}:ResourceName";


    private readonly string? _azureDirectoryName;
    private readonly string? _subscriptionId;
    private readonly string? _resourceGroupName;
    private readonly string? _resourceName;

    public ApplicationInsightsLink(IConfiguration configuration)
    {
        _azureDirectoryName = configuration[AzureDirectoryNameKey];
        _subscriptionId = configuration[SubscriptionIdKey];
        _resourceGroupName = configuration[ResourceGroupNameKey];
        _resourceName = configuration[ResourceNameKey];

        HasLink = !string.IsNullOrWhiteSpace(_azureDirectoryName) &&
            !string.IsNullOrWhiteSpace(_subscriptionId) &&
            !string.IsNullOrWhiteSpace(_resourceGroupName) &&
            !string.IsNullOrWhiteSpace(_resourceName);
    }

    public bool HasLink { get; }


    public Uri? GetLink(string? key, string? value)
    {
        if (!HasLink)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var builder = new UriBuilder(BaseUrl)
        {
            Fragment = $"#@{_azureDirectoryName}/resource/subscriptions/{_subscriptionId}/resourceGroups/{_resourceGroupName}/providers/microsoft.insights/components/{_resourceName}/transactions"
        };

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["query"] = $"{key}:{value}";
        builder.Query = query.ToString();

        return builder.Uri;
    }
}
