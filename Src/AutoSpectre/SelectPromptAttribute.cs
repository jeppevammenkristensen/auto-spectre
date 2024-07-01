using System;

namespace AutoSpectre;

[AttributeUsage(AttributeTargets.Constructor)]
public class UsedConstructorAttribute : Attribute
{
    
}

[AttributeUsage(AttributeTargets.Property)]
public class SelectPromptAttribute : AutoSpectreStepAttribute
{
    /// <summary>
    /// The converter used to convert the type of to string
    /// It is used for custom display of the select items.
    /// This can be omitted if you have a method matching
    /// {PropertyName}Converter
    /// </summary>
    public string? Converter {get;set;}
    
    /// <summary>
    /// The source of enumerable items to be used for the select prompt
    /// The source can be either a property or parameter less method
    /// It can be omitted if you have a matching property or method that matches
    /// {PropertyName}Source
    /// </summary>
    public string? Source { get; set; }
    
    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    ///  Gets or sets a value indicating whether the selection should wrap around when reaching the edge.
    /// If you meet the last item you can navigate to the first item and vice versa by going up or down
    /// </summary>
    public bool WrapAround { get; set; }

    /// <summary>
    /// Gets or sets the text to display when there are more choices available.
    /// </summary>
    public string? MoreChoicesText { get; set; }


    /// <summary>
    /// This is only relevant when the type of property is some kind of enumerable. It the instruction text
    /// for how to select multiple items
    /// </summary>
    public string? InstructionsText { get; set; }

    /// <summary>
    /// The style to for highlighting the currently selected item
    /// </summary>
    public string? HighlightStyle { get; set; }
    
    /// <summary>
    /// Enable search in the prompt. So it's possible to limit
    /// the selection choices
    /// </summary>
    public bool SearchEnabled { get; set; }
    
    /// <summary>
    /// Set this to override the default placeholder text displayed for search
    /// </summary>
    public string? SearchPlaceholderText { get; set; }
}