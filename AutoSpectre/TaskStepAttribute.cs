using System;
using Spectre.Console;

namespace AutoSpectre;

[AttributeUsage(AttributeTargets.Method)]
public class TaskStepAttribute : AutoSpectreStepAttribute
{
    /// <summary>
    /// If this is true, the method call will be wrapped in a status spinner
    /// </summary>
    public bool UseStatus { get; set; }

    /// <summary>
    /// The text to display in the status spinner. This property is ignored if <see cref="UseStatus"/> is false
    /// </summary>
    public string? StatusText { get; set; }

    /// <summary>
    /// The style to apply to the status spinner. This property is ignored if <see cref="UseStatus"/> is false
    /// </summary>
    public string? SpinnerStyle { get; set; }

    /// <summary>
    /// The type of spinner to user. Currently only Known spinner types are supported . This property is ignored if <see cref="UseStatus"/> is false
    /// <remarks>Based on command prompt the correct spinner might not be rendered. See <see href="https://github.com/spectreconsole/spectre.console/issues/391"></see>
    /// Maybe changing the font and/or adding the below in the start of the code can fix this.
    /// <code>
    /// System.Console.OutputEncoding = Encoding.UTF8;
    /// System.Console.InputEncoding = Encoding.UTF8;
    /// </code>
    ///
    /// </remarks>
    /// </summary>
    public SpinnerKnownTypes SpinnerType { get; set; } 
   
}