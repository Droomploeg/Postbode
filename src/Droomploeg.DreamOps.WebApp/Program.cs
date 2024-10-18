using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Droomploeg.DreamOps.WebApp.Components;
using Droomploeg.DreamOps.WebApp.Configurations;
using Droomploeg.DreamOps.WebApp.Middleware;
using Droomploeg.DreamOps.WebApp.Security;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var azureKeyVaultUri = configuration["AzureKeyVault"];
var credentialOptions = new DefaultAzureCredentialOptions
{
    ManagedIdentityClientId = configuration["Azure_Client_Id"],
};

try
{
    if (!string.IsNullOrWhiteSpace(azureKeyVaultUri))
    {
        builder.Configuration.AddAzureKeyVault(new Uri(azureKeyVaultUri), new DefaultAzureCredential(credentialOptions), new KeyVaultSecretManager());
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error while adding Azure Key Vault: {ex.Message}");
    throw;
}

builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, "AzureEntra");
builder.Services.AddWorkerHostedServices();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

builder.Services.AddAzureServiceBus(builder.Configuration);
builder.Services.AddApplicationCore();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();
app.UseMiddleware<ServiceBusClientContextMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.UseAuthentication();
app.UseAuthorization();

app.MapLoginAndLogout();

app.Run();
