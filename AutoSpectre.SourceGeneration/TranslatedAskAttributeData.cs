using AutoSpectre.SourceGeneration.Models;

namespace AutoSpectre.SourceGeneration;

public class TranslatedAskAttributeData
{
    public TranslatedAskAttributeData(AskTypeCopy askType, string? selectionSource, string title, string? converter, string? validator, string? condition, bool conditionNegated)
    {
        AskType = askType;
        SelectionSource = selectionSource;
        Title = title;
        Converter = converter;
        Validator = validator;
        Condition = condition;
        ConditionNegated = conditionNegated;
    }

    public string Title { get;  }
    public string? Converter { get; }
    public AskTypeCopy AskType { get;  }
    public string? SelectionSource { get; }
    public string? Validator { get; set; }
    
    public string? Condition { get; set; }
    public bool ConditionNegated { get; }
}