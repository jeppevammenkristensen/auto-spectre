namespace AutoSpectre.SourceGeneration.Evaluation;

public class ConfirmedDefaultValue
{
    public DefaultValueType Type { get; }
    
    public string Name { get; }
    

    public bool Instance { get; set; }

    public ConfirmedDefaultValue(DefaultValueType type, string name, string? style, bool instance)
    {
        Type = type;
        Name = name;
        Instance = instance;
    }
}