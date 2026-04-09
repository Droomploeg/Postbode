using Bunit;
using Bunit.TestDoubles;
using Droomploeg.DreamOps.Application.ServiceBus.Services;
using Droomploeg.DreamOps.Domain.ServiceBus.Types;
using Droomploeg.DreamOps.WebApp.Components.Pages;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace Droomploeg.DreamOps.Aspire.Tests.Actors;

public class HomePageActor
{
    private readonly IRenderedComponent<Home> _page;
    private readonly FakeNavigationManager _navigationManager;

    private HomePageActor(IRenderedComponent<Home> page, FakeNavigationManager navigationManager)
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

    public static HomePageActor Create(TestContext context, params string[] connectionNames)
    {
        var connections = connectionNames.Select(n => new ServiceBusConnection(n)).ToArray();
        context.Services.AddSingleton<IConnectionService>(new TestConnectionService(connections));
        context.Services.AddDataProtection();
        context.Services.AddSingleton<ProtectedSessionStorage>();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var navigationManager = context.Services.GetRequiredService<FakeNavigationManager>();
        var page = context.RenderComponent<Home>();
        return new HomePageActor(page, navigationManager);
    }

    private class TestConnectionService(ServiceBusConnection[] connections) : IConnectionService
    {
        public ServiceBusConnection[] Connections => connections;
    }
}
