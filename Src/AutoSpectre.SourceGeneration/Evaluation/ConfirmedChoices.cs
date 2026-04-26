using AutoSpectre.SourceGeneration.BuildContexts;

namespace AutoSpectre.SourceGeneration.Evaluation;

public class ConfirmedChoices : ISummaryCondition
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

    public void WriteToSummary(SummaryLineWriter builder)
    {
        builder.Append($" Choices: {SourceName}", true);
        if (InvalidErrorText is { })
        {
            builder.Append($" InvalidErrorText: {InvalidErrorText}");    
        }
        
        builder.NewLine();
    }
}