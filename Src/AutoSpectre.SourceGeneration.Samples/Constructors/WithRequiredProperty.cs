namespace AutoSpectre.SourceGeneration.Samples;

[AutoSpectreForm]
public class WithRequiredProperty
{
    public required string Somedependency { get; set; }
    
    [TextPrompt(Title = "Last Name")]
    public string LastName { get; set; }
}