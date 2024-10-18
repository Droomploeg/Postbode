using Droomploeg.DreamOps.Infrastructure.AzureServiceBus;
using Microsoft.AspNetCore.Components.Authorization;

namespace Droomploeg.DreamOps.WebApp.Middleware;

public class ServiceBusClientContextMiddleware
{
    private readonly RequestDelegate _next;

    public ServiceBusClientContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IServiceBusClientContext clientContext)
    {
        if (IsStaticFileRequest(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (IsUserLoggedIn(context) == false)
        {
            if (context.Request.Path == PageConstants.LoginPath ||
                context.Request.Path == PageConstants.LogoutPath ||
                context.Request.Path == PageConstants.HomePage ||
                context.Request.Path == "/signin-oidc")
            {
                await _next(context);
                return;
            }

            context.Response.Redirect(PageConstants.HomePage);
            return;
        }

        if (HasClientContext(clientContext) == false)
        {
            if (context.Request.Path != PageConstants.HomePage)
            {
                context.Response.Redirect(PageConstants.HomePage);
                return;
            }
        }

        await _next(context);
    }

    private static bool IsUserLoggedIn(HttpContext context)
    {
        return context.User.Identity?.IsAuthenticated == true;
    }

    private static bool IsStaticFileRequest(string path)
    {
        return path.Contains('.') || path.StartsWith("/_blazor");
    }

    private static bool HasClientContext(IServiceBusClientContext clientContext)
    {
        return clientContext != null && !string.IsNullOrWhiteSpace(clientContext.CurrentClient);
    }
}
