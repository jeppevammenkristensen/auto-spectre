namespace AutoSpectre.SourceGeneration.Evaluation;

public class ConfirmedChoices
{
    public string SourceName { get; }
    public ChoiceSourceType SourceType { get; }
    public string? InvalidErrorText { get; }
    public bool IsStatic { get; }

    public ConfirmedChoices(
        string sourceName, 
        ChoiceSourceType sourceType,
        string? invalidErrorText, 
        bool isStatic)
    {
        SourceName = sourceName;
        SourceType = sourceType;
        InvalidErrorText = invalidErrorText;
        IsStatic = isStatic;
    }
}