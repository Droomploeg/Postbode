namespace Droomploeg.DreamOps.WebApp.Components.Controls.AzureServiceBus.Models;

public class CustomPropertyModel
{
    public string Key { get; set; } = string.Empty;
    public CustomerPropertyDataType DataType { get; set; } = CustomerPropertyDataType.Text;

    public decimal? NumberValue { get; set; }
    public bool BooleanValue { get; set; }
    public DateOnly? DateOnly { get; set; }
    public string TextValue { get; set; } = string.Empty;

}
