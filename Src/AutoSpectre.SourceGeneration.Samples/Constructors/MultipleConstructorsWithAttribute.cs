namespace AutoSpectre.SourceGeneration.Samples;

[AutoSpectreForm]
public class MultipleConstructorsWithAttribute
{
    private readonly string _name;

    public MultipleConstructorsWithAttribute()
    {
        
    }

    [UsedConstructor]
    public MultipleConstructorsWithAttribute(string name)
    {
        _name = name;
    }
    
    [TextPrompt]
    public string FirstName { get; set; }
}