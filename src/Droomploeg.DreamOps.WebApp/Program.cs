using Droomploeg.DreamOps.Domain.ServiceBus.Models;
using Droomploeg.DreamOps.WebApp.Components;
using Droomploeg.DreamOps.WebApp.Configurations;
using Droomploeg.DreamOps.WebApp.Security;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var tenantId = configuration["AzureEntra:TenantId"];
var clientId = configuration["AzureEntra:ClientId"];
var clientSecret = configuration["AzureEntra:ClientSecret"];


// entra
// app registration

// Expose an API
// API URI: api://2e6735ba-f881-476f-8791-87a3a2209864/user_impersonation
// Admins only
// displayname: Access as a user
// Admin consent description: Access groups on behalf of the logged in user

// API Permission: 
// add Microsoft.Graph -> User.Read (delegated)
// add Azure Service Bus -> Microsoft.ServiceBus user_impersonation (delegated)
// grant admin consent


//var azureKeyVaultUri = configuration["AzureKeyVault"];
//var credentialOptions = new DefaultAzureCredentialOptions
//{
//    ManagedIdentityClientId = configuration["Azure_Client_Id"],
//};

//try
//{
//    if (!string.IsNullOrWhiteSpace(azureKeyVaultUri))
//    {
//        builder.Configuration.AddAzureKeyVault(new Uri(azureKeyVaultUri), new DefaultAzureCredential(credentialOptions), new KeyVaultSecretManager());
//    }
//}
//catch (Exception ex)
//{
//    Console.WriteLine($"Error while adding Azure Key Vault: {ex.Message}");
//    throw;
//}

builder.Services
    .AddDistributedMemoryCache()
    .AddSession()
    .AddSecurityServices(builder.Configuration)
    .AddAzureServiceBus(builder.Configuration)
    .AddWorkerHostedServices()
    .AddApplicationCore()
    .AddCommandCore()
    .AddApplicationInsightsTelemetry();

builder.Host.UseDefaultServiceProvider(o =>
{
    o.ValidateOnBuild = true;
    o.ValidateScopes = true;
});

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseSession();
app.MapPost("/connection", (HttpContext ctx, [FromBody] string clientName) =>
{
    ctx.Session.SetString(nameof(ServiceBusConnectionInfo), clientName);
    return Results.Ok();
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.UseAuthentication();
app.UseAuthorization();
//app.UseAuditEnrichment();

app.MapLoginAndLogout();

app.Run();
