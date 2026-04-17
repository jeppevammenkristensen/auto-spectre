namespace AutoSpectre.SourceGeneration.Evaluation;

public class ConfirmedChoiceStyle
{
    public ConfirmedChoiceStyle(string style)
    {
        Style = style;
    }

    public string Style { get;  }
}

public class ConfirmedCancelResult
{
    public ConfirmedCancelResult(string name, CancelResultType type, bool isStatic)
    {
        Name = name;
        Type = type;
        IsStatic = isStatic;
    }

    public string Name { get; }
    public CancelResultType Type { get; }
    public bool IsStatic { get; }
}

public enum CancelResultType
{
    Value,
    Method
}