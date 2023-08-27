﻿using AutoSpectre.SourceGeneration.Models;

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

    public static TranslatedAttributeData SelectPrompt(string title, string? selectionSource, string? converter,
        string? condition, bool conditionNegated, int? pageSize, bool? wrapAround, string? moreChoicesText,
        string? instructionsText, string? highlightStyle)
    {
        return new(askType: AskTypeCopy.Selection,
            selectionSource: selectionSource,
            title: title,
            converter: converter,
            validator: null,
            condition: condition,
            conditionNegated: conditionNegated
            )
        {
            PageSize = pageSize,
            WrapAround = wrapAround,
            MoreChoicesText = moreChoicesText,
            InstructionsText = instructionsText,
            HighlightStyle = highlightStyle
        };
    }

    public static TranslatedAttributeData TaskPrompt(string title, string? condition, bool conditionNegated, bool useStatus, string? statusText, string? spinnerStyle,SpinnerKnownTypesCopy? spinnerType)
    {
        return new TranslatedAttributeData(AskTypeCopy.Task, null, title, null, null, condition, conditionNegated)
        {
            UseStatus = useStatus,
            StatusText = statusText,
            SpinnerStyle = spinnerStyle,
            SpinnerType = spinnerType,

        };
    }

    public SpinnerKnownTypesCopy? SpinnerType { get; set; }

    public string? SpinnerStyle { get; set; }

    public string? StatusText { get; set; }

    public bool UseStatus { get; set; }

    public string? HighlightStyle { get; set; }

    public string? InstructionsText { get; set; }

    public string? MoreChoicesText { get; set; }

    public bool? WrapAround { get; set; }

    public int? PageSize { get; set; }

}