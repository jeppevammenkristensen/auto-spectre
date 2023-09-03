using System;
using Spectre.Console;

namespace AutoSpectre;

[AttributeUsage(AttributeTargets.Property)]
public class TextPromptAttribute : AutoSpectreStepAttribute
{
    /// <summary>
    /// A reference to a validator method. The method must return a
    /// string (nullable is preferable). If the result is not empty it will
    /// be considered to be an validation error otherwise not.
    ///
    /// If the property types is an enumerable the method must consist of two parameters
    /// The first must be a list of the singular type and the second a single item.
    /// For instance if you have
    /// <code>
    /// [TextPrompt(Validator = nameof(Validation)]
    /// public List&lt;int&gt; Ages {get;set;}
    /// </code>
    ///
    /// Then the method should be
    /// <code>public string? Validation(List&lt;int&gt; items, int item) { ... }</code>
    ///
    /// If the property type is not an enumerable the method must of a single parameter matching the property type
    ///
    /// By convention if you have a method fulfilling the requirements above you can omit the Validator property
    /// if the name is {PropertyName}Validator
    /// </summary>
    public string? Validator { get; set; }

    // Hide the inputted text
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
    /// The style to use for the prompting where the user inputs data
    /// </summary>
    public string? PromptStyle { get; set; }

    /// <summary>
    /// This method is only relevant if the property type is a type that is also
    /// decorated with the <see cref="AutoSpectreFormAttribute"/>. It is used to
    /// initialize an instance of the decorated property type. It is not needed
    /// if the property type has an empty constructor. You can leave this out if you
    /// have a method called Init{TypeName} 
    /// </summary>
    public string? TypeInitializer { get; set; }
}