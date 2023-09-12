namespace AutoSpectre.SourceGeneration.Evaluation;

public class ConfirmedChoices
{
    public string SourceName { get; }
    public ChoiceSourceType SourceType { get; }
    public string? Style { get; }
    public string? InvalidErrorText { get; }

    public ConfirmedChoices(string sourceName, ChoiceSourceType sourceType, string? style, string? invalidErrorText )
    {
        SourceName = sourceName;
        SourceType = sourceType;
        Style = style;
        InvalidErrorText = invalidErrorText;
    }
}