using System;
using System.Text;
using AutoSpectre.SourceGeneration.BuildContexts;

namespace AutoSpectre.SourceGeneration.Evaluation;

/// <summary>
/// A condition that can write information to the a summary
/// </summary>
public interface ISummaryCondition
{
    void WriteToSummary(SummaryLineWriter builder);
}

public class ConfirmedCondition : ISummaryCondition
{
    public ConfirmedCondition(string condition, ConditionSource sourceType, bool negate)
    {
        Condition = condition;
        SourceType = sourceType;
        Negate = negate;
    }

    public string Condition { get; set; }
    public ConditionSource SourceType { get; set; }
    public bool Negate { get; }
    public void WriteToSummary(SummaryLineWriter builder)
    {
        builder.Append($"/// Condition: {Condition}");
        if (Negate)
        {
            builder.Append($" Negated: {Negate}");    
        }

        builder.NewLine();
    }
}

