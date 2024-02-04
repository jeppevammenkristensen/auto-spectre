namespace AutoSpectre.SourceGeneration.Samples;

[AutoSpectreForm]
public class NoConstructor
{
    [TextPrompt]
    public string FirstName { get; set; }
}

[AutoSpectreForm]
public class WithRequiredProperty
{
    public required string Somedependency { get; set; }
    
    [TextPrompt(Title = "Last Name")]
    public string LastName { get; set; }
}