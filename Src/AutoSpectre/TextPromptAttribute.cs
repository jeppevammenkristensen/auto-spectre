using System;

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
    /// This will be applied in both a TextPrompt but also on a ConfirmationPrompt
    /// </summary>
    public string? DefaultValueStyle { get; set; }

    /// <summary>
    /// The value to choose as default value. The source must be public
    /// and available in the class. Static or instance and be available through one of the
    /// below
    /// * A field 
    /// * A method with no parameters
    /// * A property
    ///  </summary>
    public string? DefaultValueSource { get; set; }

    /// <summary>
    /// The style to use for the prompting where the user inputs data. PromptStyle will not be applied
    /// if the decorated property returns bool (in which case a ConfirmationPrompt is generated, which does
    /// not support PromptStyle)
    /// </summary>
    public string? PromptStyle { get; set; }

    /// <summary>
    /// This method is only relevant if the property type is a type that is also
    /// decorated with the <see cref="AutoSpectreFormAttribute"/>. It is used to
    /// initialize an instance of the decorated property type. It is not needed
    /// if the property type has an empty constructor. You can leave this out if you
    /// have a method called Init{TypeName}. The method must be public but can be instance or static
    /// </summary>
    public string? TypeInitializer { get; set; }

    /// <summary>
    /// Use this to limit the values allowed to be entered. They will be displayed in the
    /// prompt and you be able to autocomplete with tab. The convention is {PropertyName}Choices
    /// </summary>
    public string? ChoicesSource { get; set; }

    /// <summary>
    /// The style used to display the choices in the prompt. This will be applied
    /// to both TextPrompt and ConfirmationPrompts being generated
    /// </summary>
    public string? ChoicesStyle { get; set; }

    /// <summary>
    /// This is the text that will be displayed to the user if an invalid text is used
    /// </summary>
    public string? ChoicesInvalidText { get; set; }
}