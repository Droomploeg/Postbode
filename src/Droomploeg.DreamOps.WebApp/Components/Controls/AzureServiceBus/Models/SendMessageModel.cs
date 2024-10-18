using Azure.Messaging.ServiceBus;

namespace Droomploeg.DreamOps.WebApp.Components.Controls.AzureServiceBus.Models;

public class SendMessageModel
{
    public string ContentType { get; set; } = "text/plain";
    public string Body { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string ReplyTo { get; set; } = string.Empty;
    public string ReplyToSessionId { get; set; } = string.Empty;
    public List<CustomPropertyModel> CustomProperties { get; set; } = [];

    public bool IsResubmitMessage { get; set; }

    public bool GenenerateMessageId { get; set; }

    public bool DeleteMessageAfterResubmit { get; set; }

    public ServiceBusMessage ToSendMessage()
    {
        var message = new ServiceBusMessage(Body)
        {
            ContentType = ContentType,
        };

        if (!string.IsNullOrWhiteSpace(CorrelationId))
        {
            message.CorrelationId = CorrelationId;
        }

        if (!string.IsNullOrWhiteSpace(MessageId))
        {
            message.MessageId = MessageId;
        }

        if (!string.IsNullOrEmpty(Subject))
        {
            message.Subject = Subject;
        }

        if (!string.IsNullOrEmpty(SessionId))
        {
            message.SessionId = SessionId;
        }

        if (!string.IsNullOrEmpty(ReplyTo))
        {
            message.ReplyTo = ReplyTo;
        }

        if (!string.IsNullOrEmpty(ReplyToSessionId))
        {
            message.ReplyToSessionId = ReplyToSessionId;
        }

        // todo: scheduled enqueue time

        foreach (var property in CustomProperties)
        {
            switch (property.DataType)
            {
                case CustomerPropertyDataType.Text:
                    message.ApplicationProperties.Add(property.Key, property.TextValue);
                    break;
                case CustomerPropertyDataType.Number:
                    message.ApplicationProperties.Add(property.Key, property.NumberValue);
                    break;
                case CustomerPropertyDataType.Boolean:
                    message.ApplicationProperties.Add(property.Key, property.BooleanValue);
                    break;
                case CustomerPropertyDataType.DateOnly:
                    message.ApplicationProperties.Add(property.Key, property.DateOnly);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return message;
    }
}
