namespace AutoSpectre.SourceGeneration.Evaluation;

public class ConfirmedConverter
{
    public string Converter { get; }
    public bool IsStatic { get; }

    public ConfirmedConverter(string converter, bool isStatic)
    {
        Converter = converter;
        IsStatic = isStatic;
    }
}