using Azure.Messaging.ServiceBus;
using Droomploeg.Postbode.WebApp.Common;
using Droomploeg.Postbode.WebApp.Common.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Html;
using System.Text.Json;

namespace Droomploeg.Postbode.WebApp.Components.Controls.AzureServiceBus;

public partial class ReceivedMessageControl : ComponentBase
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly Dictionary<object, object> _brokerProperties = [];
    private readonly Dictionary<object, object> _customProperties = [];

    [Parameter] public ServiceBusReceivedMessage? Message { get; set; }

    private string _body = string.Empty;

    protected override void OnParametersSet()
    {
        FormatBody();
        UpdateBrokerProperties();
        UpdateCustomerProperties();
        base.OnParametersSet();
    }

    private void UpdateCustomerProperties()
    {
        _customProperties.Clear();
        var customProperties = Message?.ApplicationProperties;
        if (customProperties == null)
        {
            return;
        }

        foreach (var keyValue in customProperties)
        {
            _customProperties.Add(keyValue.Key, keyValue.Value);
        }
    }

    private void UpdateBrokerProperties()
    {
        _brokerProperties.Clear();
        _brokerProperties.AddNotNull("Sequence Number", Message?.SequenceNumber);
        _brokerProperties.AddNotNull("Content Type", Message?.ContentType);
        _brokerProperties.AddNotNull("Subject", Message?.Subject);
        _brokerProperties.AddNotNull("Enqueue Time", Message?.EnqueuedTime);
        _brokerProperties.AddNotNull("Scheduled Enqueue Time", Message?.ScheduledEnqueueTime);
        _brokerProperties.AddNotNull("Expires At", Message?.ExpiresAt);
        _brokerProperties.AddNotNull("Time To Live", Message?.TimeToLive);
        _brokerProperties.AddNotNull("Dead Letter Error Description", Message?.DeadLetterErrorDescription);
        _brokerProperties.AddNotNull("Dead Letter Reason", Message?.DeadLetterReason);
        _brokerProperties.AddNotNull("Dead Letter Source", Message?.DeadLetterSource);
        _brokerProperties.AddNotNull("Reply To", Message?.ReplyTo);
        _brokerProperties.AddNotNull("Reply To Session Id", Message?.ReplyToSessionId);
        _brokerProperties.AddNotNull("Delivery Count", Message?.DeliveryCount);

        object? value;
        value = GetDisplayValue("correlationid", Message?.CorrelationId);
        _brokerProperties.AddNotNull("Correlation Id", value);
        value = GetDisplayValue("messageid", Message?.MessageId);
        _brokerProperties.AddNotNull("Message Id", value);
        value = GetDisplayValue("sessionid", Message?.SessionId);
        _brokerProperties.AddNotNull("Session Id", value);
    }

    public void FormatBody()
    {
        if (Message?.Body == null)
        {
            _body = string.Empty;
            return;
        }

        var contentType = Message.ContentType;
        _body = Message.Body.ToString();
        if (MessageContentType.IsJson(contentType))
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(_body);
                JsonElement root = doc.RootElement;

                _body = JsonSerializer.Serialize(root, JsonSerializerOptions);
            }
            catch { }
        }
    }

    private object? GetDisplayValue(string key, string? value)
    {
        if (!ApplicationInsightLink.HasLink)
        {
            return value;
        }

        var link = ApplicationInsightLink.GetLink(key, value);
        if (link == null)
        {
            return null;
        }

        return new MarkupString($"<a href=\"{link}\" target=\"_blank\">{value}</a>");
    }
}
