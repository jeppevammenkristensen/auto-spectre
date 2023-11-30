using System;

namespace AutoSpectre;

/// <summary>
/// Marker interface. Apply this to a class and a factory will be generated
/// based on the properties in decorated with the <see cref="AskAttribute"/>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class AutoSpectreForm : Attribute
{
    public string? Culture { get; set; }
    
    /// <summary>
    /// Enable this to not generate a Dump extension method
    /// </summary>
    public bool DisableDump { get; set; }
}