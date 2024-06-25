using System;
using System.Linq;
using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.BuildContexts;

internal class SelectionPromptBuildContext : SelectionBaseBuildContext
{
    public ITypeSymbol TypeSymbol { get; }
    public bool Nullable { get; }    

    public SelectionPromptBuildContext(string title, SinglePropertyEvaluationContext context) : base(title,context)
    {
        TypeSymbol = context.Type;
        Nullable = context.IsNullable;
        if (context.ConfirmedSelectionSource is null)
            throw new ArgumentException("The selection source cannot be null");
    }

    public override string GenerateOutput(string destination)
    {
        return $"{destination} = {PromptPart()};";
    }

    public override string PromptPart(string? variableName = null)
    {
        var type = TypeSymbol.ToDisplayString();

        return $"""
        AnsiConsole.Prompt(
        new SelectionPrompt<{type}>()
            .Title({GenerateTitleString()} ){GenerateConverter()}
            {GeneratePageSize()}
            {GenerateWrapAround()}
            {GenerateMoreChoicesText()}
            {GenerateHighlightStyle()}
            .AddChoices({GetChoicePrepend()}.{GetSelector()}.ToArray()))
        """;
    }
}