using Droomploeg.Postbode.WebApp.Components;
using Droomploeg.Postbode.WebApp.Configurations;
using Droomploeg.Postbode.WebApp.Security;

var builder = WebApplication.CreateBuilder(args);

builder.AddKeyVaultConfiguration();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services
    .AddDistributedMemoryCache()
    .AddSession()
    .AddSecurityServices(builder.Configuration)
    .AddAzureServiceBus(builder.Configuration, builder.Environment)
    .AddWorkerHostedServices()
    .AddApplicationCore()
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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.UseAuthentication();
app.UseAuthorization();

app.MapLoginAndLogout();

app.Run();
