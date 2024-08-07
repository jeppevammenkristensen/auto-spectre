﻿using System;
using System.Text;
using AutoSpectre.SourceGeneration.Evaluation;
using AutoSpectre.SourceGeneration.Extensions;

namespace AutoSpectre.SourceGeneration.BuildContexts;

public class ConfirmPromptBuildContext : PromptBuildContext
{
    public bool IsNullable { get; }

    public ConfirmPromptBuildContext(string title, bool isNullable,
        SinglePropertyEvaluationContext evaluationContext) : base(evaluationContext, title)
    {
        IsNullable = isNullable;
    }

    public override string GenerateOutput(string destination)
    {
        return $"""{destination} = {PromptPart()};""";
    }

    public override string PromptPart(string? variableName = null)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"""AnsiConsole.Prompt(new ConfirmationPrompt({GenerateTitleString()})""");
        BuildDefaultStyle(stringBuilder);
        BuildChoicesStyle(stringBuilder);
        stringBuilder.AppendLine(")");
        return stringBuilder.ToString();
    }
    
    private void BuildDefaultStyle(StringBuilder builder)
    {
        if (Context.ConfirmedDefaultStyle is { Style: {} style } _)
        {
            builder.AppendLine($".DefaultValueStyle({style.GetSafeTextWithQuotes()})");
        }
    }

    private void BuildChoicesStyle(StringBuilder builder)
    {
        if (Context.ConfirmedChoicesStyle is { } style)
        {
            builder.AppendLine($".ChoicesStyle({style.Style.GetSafeTextWithQuotes()})");
        }
    }
  
}