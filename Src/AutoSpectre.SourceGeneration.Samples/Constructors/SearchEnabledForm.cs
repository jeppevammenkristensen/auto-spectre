namespace AutoSpectre.SourceGeneration.Samples;

[AutoSpectreForm]
public class SearchEnabledForm
{
    [TextPrompt(SearchEnabled = true, SearchPlaceholderText = "Reduce selection \"name\"")]
    public SearchEnabledEnum SearchEnum { get; set; }
    
    [SelectPrompt(Source = nameof(SearchStringSource), SearchEnabled = true, SearchPlaceholderText = "Placeholder \"text\"")]
    public string SearchString { get; set; }

    public string[] SearchStringSource() => new[] {"First", "Second"};
}