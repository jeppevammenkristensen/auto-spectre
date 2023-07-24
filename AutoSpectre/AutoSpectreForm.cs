using System;

namespace AutoSpectre;

/// <summary>
/// Marker interface. Apply this to a class and a factory will be generated
/// based on the properties in decorated with the <see cref="AskAttribute"/>
/// </summary>
public class AutoSpectreForm : Attribute
{
    
}