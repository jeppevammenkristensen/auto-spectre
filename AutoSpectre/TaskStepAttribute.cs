using System;

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
   
}