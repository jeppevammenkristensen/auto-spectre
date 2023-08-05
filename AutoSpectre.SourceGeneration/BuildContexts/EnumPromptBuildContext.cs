using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.BuildContexts;

public class EnumPromptBuildContext : PromptBuildContext
{
    public string Title { get; }
    public ITypeSymbol Type { get; }
    public bool IsNullable { get; }

    public EnumPromptBuildContext(string title, ITypeSymbol type, bool isNullable,
        SinglePropertyEvaluationContext context) : base(context)
    {
        Title = title;
        Type = type;
        IsNullable = isNullable;
    }

    public override string GenerateOutput(string destination)
    {
        return $"{destination} = {PromptPart(null)};    ";
    }

    public override string PromptPart(string? variableName = null)
    {
        var type = Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        return $"""
        AnsiConsole.Prompt(
        new SelectionPrompt<{type}>()
            .Title("{Title}")
            .PageSize(10) 
            .AddChoices(Enum.GetValues<{type}>()))
        """;
    }
}