using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.BuildContexts;

public class EnumPromptBuildContext : PromptBuildContext
{
   
    public ITypeSymbol Type { get; }
    public bool IsNullable { get; }

    public EnumPromptBuildContext(string title, ITypeSymbol type, bool isNullable,
        SinglePropertyEvaluationContext context) : base(context, title)
    {
        Type = type;
        IsNullable = isNullable;
        TypeName = type is INamedTypeSymbol namedTypeSymbol ? namedTypeSymbol.FullName() : type.Name;
    }

    public string TypeName { get; set; }

    public override string GenerateOutput(string destination)
    {
        return $"{destination} = {PromptPart(null)};    ";
    }

    public override string PromptPart(string? variableName = null)
    {
        return $"""
        AnsiConsole.Prompt(
        new SelectionPrompt<{TypeName}>()
            .Title({GenerateTitleString()})
            .PageSize(10) 
            {Context.ConfirmedSearchEnabled.GetSearchString()}
            .AddChoices(Enum.GetValues<{TypeName}>()))
        """;
    }
}