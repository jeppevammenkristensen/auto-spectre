namespace AutoSpectre;

public class SelectPromptAttribute : AutoSpectrePropertyAttribute
{
    public string? Converter {get;set;}
    public string? Source { get; set; }
    
    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    ///  Gets or sets a value indicating whether the selection should wrap around when reaching the edge.
    /// </summary>
    public bool WrapAround { get; set; }

    /// <summary>
    /// Gets or sets the text to display when there are more choices available.
    /// </summary>
    public string? MoreChoicesText { get; set; }


    /// <summary>
    /// This is only relevant the type of property some kind of enumerable. It the instruction text
    /// for how to selct multiple items
    /// </summary>
    public string? InstructionsText { get; set; }

    /// <summary>
    /// The style to for highlighting the currently selected item
    /// </summary>
    public string? HighlightStyle { get; set; }
}