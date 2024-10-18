using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace Droomploeg.DreamOps.IntegrationsTests.Common.BUnit;

internal static class TestContextExtensions
{
    internal static IElement? GetButton(this IRenderedComponent<IComponent> page, string buttonName) 
        => page.FindAll(HtmlTags.Button, true).FirstOrDefault(b => b.TextContent == buttonName);

    internal static IElement FindById(this IRenderedComponent<IComponent> page, string id) 
        => page.Nodes.GetElementById(id) ?? throw new ElementNotFoundException("No element with Id '{id}' found");
}
