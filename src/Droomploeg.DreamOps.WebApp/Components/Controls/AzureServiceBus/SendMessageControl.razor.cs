using Azure.Messaging.ServiceBus;
using Droomploeg.DreamOps.WebApp.Components.Controls.AzureServiceBus.Models;
using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.WebApp.Components.Controls.AzureServiceBus;

public partial class SendMessageControl : ComponentBase
{
    [SupplyParameterFromForm] private SendMessageModel Model { get; set; } = new SendMessageModel();

    [Parameter] public ServiceBusReceivedMessage? Message { get; set; }

    [Parameter] public bool SessionEnabled { get; set; } = false;

    [Parameter] public EventCallback<SendMessageModel> OnSend { get; set; }

    [Parameter] public EventCallback OnCancel { get; set; }

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
        Model.CustomProperties.Add(new CustomPropertyModel());
        StateHasChanged();
    }

    private void RemoveCustomProperty(CustomPropertyModel property)
    {
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
