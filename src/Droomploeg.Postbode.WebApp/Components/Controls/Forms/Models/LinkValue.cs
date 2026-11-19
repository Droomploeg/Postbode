namespace Droomploeg.Postbode.WebApp.Components.Controls.Forms.Models;

/// <summary>
/// A value that <see cref="DictionaryControl"/> renders as an external link.
/// Both parts are rendered through Blazor's encoding, so they may safely hold
/// untrusted input such as message identifiers.
/// </summary>
/// <param name="Link">The target of the link.</param>
/// <param name="Text">The visible text of the link.</param>
public sealed record LinkValue(Uri Link, string Text);
