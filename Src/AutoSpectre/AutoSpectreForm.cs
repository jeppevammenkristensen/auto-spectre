using System;

namespace AutoSpectre;

/// <summary>
/// Marker interface. Apply this to a class and a factory will be generated
/// based on the properties the decorated properties
/// </summary>
/// <remarks>
/// Example
/// <code>
/// [AutoSpectreForm]
/// public class UserInfo
/// {
///     [TextPrompt(Title = "Enter your [green]name[/]")]
///     public string Name { get; set; } = string.Empty;
///
///     [TextPrompt(Title = "Enter your [green]age[/]")]
///     public int Age { get; set; }
///
///     [TaskStep(Title = "Processing...", UseStatus = true, StatusText = "Saving")]
///     public void Save(IAnsiConsole console)
///     {
///         // Perform work based on prompted values
///     }
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class AutoSpectreForm : Attribute
{
    /// <summary>
    /// Gets or sets the culture to use for prompts. If not set, the default culture is used.
    /// </summary>
    public string? Culture { get; set; }
    
    /// <summary>
    /// This has been retired
    /// </summary>
    [Obsolete("Retired used dumpify instead or other to output", true)]
    public bool DisableDump { get; set; }
}