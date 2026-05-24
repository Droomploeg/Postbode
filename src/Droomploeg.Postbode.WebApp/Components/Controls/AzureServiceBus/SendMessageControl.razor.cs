using Azure.Messaging.ServiceBus;
using Droomploeg.Postbode.WebApp.Components.Controls.AzureServiceBus.Models;
using Microsoft.AspNetCore.Components;

namespace Droomploeg.Postbode.WebApp.Components.Controls.AzureServiceBus;

public partial class SendMessageControl : ComponentBase
{
    [SupplyParameterFromForm] private SendMessageModel? Model { get; set; }

    [Parameter] public ServiceBusReceivedMessage? Message { get; set; }

    [Parameter] public bool SessionEnabled { get; set; } = false;

    [Parameter] public EventCallback<SendMessageModel> OnSend { get; set; }

    [Parameter] public EventCallback OnCancel { get; set; }
    
    private bool EnableSendButton => (!SessionEnabled || !string.IsNullOrWhiteSpace(SessionId)) && !string.IsNullOrWhiteSpace(ContentType) && !string.IsNullOrWhiteSpace(Body);

    protected override void OnInitialized()
    {
        Model = new SendMessageModel();
        base.OnInitialized();
    }

    private bool GenerateMessageId
    {
        get => Model?.GenerateMessageId ?? true;
        set
        {
            Model ??= new SendMessageModel();
            Model.GenerateMessageId = value;
        }
    }
    
    private bool DeleteMessageAfterResubmit
    {
        get => Model?.DeleteMessageAfterResubmit ?? false;
        set
        {
            Model ??= new SendMessageModel();
            Model.DeleteMessageAfterResubmit = value;
        }
    }
    
    private string ContentType
    {
        get => Model?.ContentType ?? string.Empty;
        set
        {
            Model ??= new SendMessageModel();
            Model.ContentType = value;
        }
    }
    
    private string Subject
    {
        get => Model?.Subject ?? string.Empty;
        set
        {
            Model ??= new SendMessageModel();
            Model.Subject = value;
        }
    }

    private string Body
    {
        get => Model?.Body ?? string.Empty;
        set
        {
            Model ??= new SendMessageModel();
            Model.Body = value;
        }
    }
    
    private string CorrelationId
    {
        get => Model?.CorrelationId ?? string.Empty;
        set
        {
            Model ??= new SendMessageModel();
            Model.CorrelationId = value;
        }
    }
    
    private string MessageId
    {
        get => Model?.MessageId ?? string.Empty;
        set
        {
            Model ??= new SendMessageModel();
            Model.MessageId = value;
        }
    }
    
    private string SessionId
    {
        get => Model?.SessionId ?? string.Empty;
        set
        {
            Model ??= new SendMessageModel();
            Model.SessionId = value;
        }
    }
    
    private string ReplyTo
    {
        get => Model?.ReplyTo ?? string.Empty;
        set
        {
            Model ??= new SendMessageModel();
            Model.ReplyTo = value;
        }
    }

    private string ReplyToSessionId
    {
        get => Model?.ReplyToSessionId ?? string.Empty;
        set
        {
            Model ??= new SendMessageModel();
            Model.ReplyToSessionId = value;
        }
    }
    
    private async Task SendAsync()
    {
        if (OnSend.HasDelegate)
        {
            await OnSend.InvokeAsync(Model);
        }

        Model = new SendMessageModel();
        StateHasChanged();
    }

    private async Task CancelAsync()
    {
        if (OnCancel.HasDelegate)
        {
            await OnCancel.InvokeAsync();
        }
    }

    protected override void OnParametersSet()
    {
        if (Message is not null)
        {
            Model ??= new SendMessageModel();
            Model.Subject = Message.Subject;
            Model.Body = Message.Body.ToString();
            Model.ContentType = Message.ContentType;
            Model.CorrelationId = Message.CorrelationId;
            Model.MessageId = Message.MessageId;
            Model.SessionId = Message.SessionId;
            Model.ReplyTo = Message.ReplyTo;
            Model.ReplyToSessionId = Message.ReplyToSessionId;
            Model.MessageId = Message.MessageId;
            Model.SessionId = Message.SessionId;
            Model.ReplyTo = Message.ReplyTo;
            Model.ReplyToSessionId = Message.ReplyToSessionId;
            Model.CustomProperties = Message.ApplicationProperties
                .Where(x => !DeadLetterInfo(x.Key))
                .Select(ToCustomPropertyModel)
                .ToList();
        }

        base.OnParametersSet();
    }

    private void AddCustomProperty()
    {
        Model ??= new SendMessageModel();
        Model.CustomProperties.Add(new CustomPropertyModel());
        StateHasChanged();
    }

    private void RemoveCustomProperty(CustomPropertyModel property)
    {
        Model ??= new SendMessageModel();
        Model.CustomProperties.Remove(property);
        StateHasChanged();
    }

    private void ChangeDataType(CustomPropertyModel property, ChangeEventArgs e)
    {
        property.DataType = Enum.Parse<CustomerPropertyDataType>(e.Value!.ToString()!);
        StateHasChanged();
    }

    private static bool DeadLetterInfo(string key) =>
            "Diagnostic-Id".Equals(key) ||
            "DeadLetterSource".Equals(key) ||
            "DeadLetterReason".Equals(key) ||
            "DeadLetterErrorDescription".Equals(key);

    private static CustomPropertyModel ToCustomPropertyModel(KeyValuePair<string, object> keyValue)
    {
        if (keyValue.Value is DateOnly dateOnlyValue)
        {
            return new CustomPropertyModel
            {
                Key = keyValue.Key,
                DateOnly = dateOnlyValue,
                DataType = CustomerPropertyDataType.Number
            };
        }
        else if (keyValue.Value is bool boolValue)
        {
            return new CustomPropertyModel
            {
                Key = keyValue.Key,
                BooleanValue = boolValue,
                DataType = CustomerPropertyDataType.Number
            };
        }
        else if (keyValue.Value is decimal ||
            keyValue.Value is int ||
            keyValue.Value is double ||
            keyValue.Value is float)
        {
            return new CustomPropertyModel
            {
                Key = keyValue.Key,
                NumberValue = Convert.ToDecimal(keyValue.Value),
                DataType = CustomerPropertyDataType.Number
            };
        }
        else
        {
            return new CustomPropertyModel
            {
                Key = keyValue.Key,
                TextValue = keyValue.Value?.ToString() ?? string.Empty,
                DataType = CustomerPropertyDataType.Text
            };
        }
    }
}
