namespace AutoSpectre.SourceGeneration.Evaluation;

public class ConfirmedValidator
{
    public ConfirmedValidator(string name, bool singleValidation, bool isStatic)
    {
        Name = name;
        SingleValidation = singleValidation;
        IsStatic = isStatic;
    }

    public string Name { get; set; }
    public bool SingleValidation { get; }
    public bool IsStatic { get; }
}

public class ConfirmedDefaultValue
{
    public DefaultValueType Type { get; }
    
    public string Name { get; }
    public string? Style { get; }

    public bool Instance { get; set; }

    public ConfirmedDefaultValue(DefaultValueType type, string name, string? style, bool instance)
    {
        Type = type;
        Name = name;
        Style = style;
        Instance = instance;
    }
}

public enum DefaultValueType
{
    Method,
    Property
}