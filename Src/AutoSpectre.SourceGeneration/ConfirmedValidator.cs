namespace AutoSpectre.SourceGeneration;

public class ConfirmedValidator
{
    public ConfirmedValidator(string name, bool singleValidation)
    {
        Name = name;
        SingleValidation = singleValidation;
    }

    public string Name { get; set; }
    public bool SingleValidation { get; }
}

public class ConfirmedDefaultValue
{
    public DefaultValueType Type { get; }
    public string Name { get; }
    public string? Style { get; }

    public ConfirmedDefaultValue(DefaultValueType type, string name, string? style)
    {
        Type = type;
        Name = name;
        Style = style;
    }
}

public enum DefaultValueType
{
    Literal,
    Call
}