using System;

namespace AutoSpectre;

public class AutoSpectrePropertyAttribute : Attribute
{
    /// <summary>
    /// The title to display for the prompt. If not specified, the following
    /// will be used:
    /// Enter [green]{PropertyName}[/]
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Indicates a reference to a condition that determines whether a prompt should
    /// be displayed for the given property. The condition can be a property or a method with no parameters
    /// that returns true 
    /// </summary>
    public string? Condition { get; set; }


    /// <summary>
    /// Should be used with the <see cref="Condition"/> property.
    /// if true it will negate the result of the condition. Can be used
    /// in if true do that if false do some other thing
    /// </summary>
    public bool NegateCondition { get; set; }
}