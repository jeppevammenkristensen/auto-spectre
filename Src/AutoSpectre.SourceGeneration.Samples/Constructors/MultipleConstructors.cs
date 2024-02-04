namespace AutoSpectre.SourceGeneration.Samples;

[AutoSpectreForm]
public class MultipleConstructors
{
    private readonly string _name;

    public MultipleConstructors()
    {
        
    }

    public MultipleConstructors(string name)
    {
        _name = name;
    }
    
    [TextPrompt]
    public string FirstName { get; set; }
}