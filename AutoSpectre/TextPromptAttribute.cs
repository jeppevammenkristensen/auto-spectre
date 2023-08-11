using Spectre.Console;

namespace AutoSpectre;

public class TextPromptAttribute : AutoSpectrePropertyAttribute
{
    public string? Validator { get; set; }

    // Retrieve the field as a secret.
    public bool Secret { get; set; }

    /// <summary>
    /// The mask to use. (The default is *). Is only relevant if <see cref="Secret"/> is set to true
    /// </summary>
    public char Mask { get; set; } = '*';
    
    /// <summary>
    /// The style to apply to the display of the default value
    /// Note. To enable a DefaultValue use this style to declare the property like this
    /// public string SomeProperty { get;set;} = "Default value";
    /// </summary>
    public string? DefaultValueStyle { get; set; }

    /// <summary>
    /// The style to use for the prompt
    /// </summary>
    public string? PromptStyle { get; set; }
}