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
}