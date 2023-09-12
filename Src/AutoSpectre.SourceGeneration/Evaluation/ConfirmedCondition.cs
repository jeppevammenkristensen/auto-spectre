namespace AutoSpectre.SourceGeneration.Evaluation;

public class ConfirmedCondition
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
}

