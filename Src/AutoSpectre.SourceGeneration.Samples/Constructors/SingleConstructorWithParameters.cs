namespace AutoSpectre.SourceGeneration.Samples;

[AutoSpectreForm]
public class SingleConstructorWithParameters
{
    public SingleConstructorWithParameters(string name)
    {
        
    }
    
    [TextPrompt]
    public string Name { get; set; }
    
}