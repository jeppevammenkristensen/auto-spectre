using System;
using System.Text;
using AutoSpectre.SourceGeneration.Evaluation;

namespace AutoSpectre.SourceGeneration.BuildContexts;

public class ConfirmPromptBuildContext : PromptBuildContext
{
    public string Title { get; }
    public bool IsNullable { get; }

    public ConfirmPromptBuildContext(string title, bool isNullable,
        SinglePropertyEvaluationContext evaluationContext) : base(evaluationContext)
    {
        Title = title;
        IsNullable = isNullable;
    }

    public override string GenerateOutput(string destination)
    {
        return $"""{destination} = {PromptPart()};""";
    }

    public override string PromptPart(string? variableName = null)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"""AnsiConsole.Prompt(new ConfirmationPrompt("{Title}")""");
        BuildDefaultStyle(stringBuilder);
        BuildChoicesStyle(stringBuilder);
        stringBuilder.AppendLine(")");
        return stringBuilder.ToString();
    }
    
    private void BuildDefaultStyle(StringBuilder builder)
    {
        if (Context.ConfirmedDefaultStyle is { Style: {} style } _)
        {
            builder.AppendLine($".DefaultValueStyle(\"{style}\")");
        }
    }

    private void BuildChoicesStyle(StringBuilder builder)
    {
        if (Context.ConfirmedChoicesStyle is { } style)
        {
            builder.AppendLine($".ChoicesStyle(\"{style.Style}\")");
        }
    }
  
}