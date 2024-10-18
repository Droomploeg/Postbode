namespace Droomploeg.DreamOps.WebApp.Components.Controls.AzureServiceBus.Models;

public class DeadLetterMessageModel
{
    public string Reason { get; set; } = null!;
    public string Description { get; set; } = null!;
}
