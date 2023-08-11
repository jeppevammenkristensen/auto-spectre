using AutoSpectre.SourceGeneration.Models;

namespace AutoSpectre.SourceGeneration;

public class TranslatedAttributeData
{
    public TranslatedAttributeData(AskTypeCopy askType, string? selectionSource, string title, string? converter, string? validator, string? condition, bool conditionNegated)
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

    public static TranslatedAttributeData TextPrompt(string title, string? validator,
        string? condition, bool conditionNegated, bool secret, char? mask, string? defaultValueStyle,
        string? promptStyle)
    {
        return new(askType: AskTypeCopy.Normal, selectionSource: null, title: title, converter: null, validator: validator, condition: condition, conditionNegated: conditionNegated)
        {
            Secret = secret,
            Mask = mask,
            DefaultValueStyle = defaultValueStyle,
            PromptStyle = promptStyle
        };
    }

    public string? PromptStyle { get; set; }

    public string? DefaultValueStyle { get; set; }

    public char? Mask { get; set; }

    public bool Secret { get; set; }

    public static TranslatedAttributeData SelectPrompt(string title, string? selectionSource, string? converter, string? condition, bool conditionNegated, int? pageSize, bool? wrapAround)
    {
        return new(askType: AskTypeCopy.Selection,
            selectionSource: selectionSource,
            title: title,
            converter: converter,
            validator: null,
            condition: condition,
            conditionNegated: conditionNegated)
        {
            PageSize = pageSize,
            WrapAround = wrapAround
        };
    }

    public bool? WrapAround { get; set; }

    public int? PageSize { get; set; }
}