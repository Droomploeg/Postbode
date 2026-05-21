using Bunit;
using Bunit.TestDoubles;
using Droomploeg.DreamOps.Application.ServiceBus.Services;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Droomploeg.DreamOps.WebApp.Components.Pages;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Droomploeg.DreamOps.Aspire.Tests.Actors;

public class HomePageActor
{
    private readonly IRenderedComponent<Home> _page;
    private readonly BunitNavigationManager _navigationManager;

    private HomePageActor(IRenderedComponent<Home> page, BunitNavigationManager navigationManager)
    {
        _page = page;
        _navigationManager = navigationManager;
    }

    public bool ContainsText(string text) =>
        _page.Markup.Contains(text);

    public bool HasConnection(string name) =>
        _page.Markup.Contains(name);

    public bool HasNavigatedTo(string path) =>
        _navigationManager.Uri.Contains(path);

    public static HomePageActor Create(BunitContext context, params string[] connectionNames)
    {
        var connections = connectionNames.Select(n => new ServiceBusConnection(n)).ToArray();
        context.Services.AddSingleton<IConnectionService>(new TestConnectionService(connections));
        context.Services.AddDataProtection();
        context.Services.AddSingleton<ProtectedSessionStorage>();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var navigationManager = context.Services.GetRequiredService<BunitNavigationManager>();
        var page = context.Render<Home>();
        return new HomePageActor(page, navigationManager);
    }

    private class TestConnectionService(ServiceBusConnection[] connections) : IConnectionService
    {
        public ServiceBusConnection[] Connections => connections;
    }
}
