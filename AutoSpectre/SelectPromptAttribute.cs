namespace AutoSpectre;

public class SelectPromptAttribute : AutoSpectrePropertyAttribute
{
    public string? Converter {get;set;}
    public string? Source { get; set; }

}