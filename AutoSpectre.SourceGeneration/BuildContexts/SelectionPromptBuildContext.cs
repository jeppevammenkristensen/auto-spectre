using System;
using System.Linq;
using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.BuildContexts;

internal class SelectionPromptBuildContext : SelectionBaseBuildContext
{
    public ITypeSymbol TypeSymbol { get; }
    public bool Nullable { get; }
    public string SelectionTypeName { get; set; }
    public SelectionPromptSelectionType SelectionType { get; }


    public SelectionPromptBuildContext(string title, SinglePropertyEvaluationContext context, string selectionTypeName, SelectionPromptSelectionType selectionType) : base(title,context)
    {
        TypeSymbol = context.Type;
        Nullable = context.IsNullable;
        SelectionTypeName = selectionTypeName ?? throw new ArgumentNullException(nameof(selectionTypeName));
        SelectionType = selectionType;
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
            .Title("{Title}"){GenerateConverter()}
            {GeneratePageSize()}
            {GenerateWrapAround()}
            {GenerateMoreChoicesText()}
            .AddChoices(destination.{GetSelector()}.ToArray()))
        """;
    }
    
    
   
    

    private string GetSelector() => SelectionType switch
    {
        SelectionPromptSelectionType.Method => $"{SelectionTypeName}()",
        SelectionPromptSelectionType.Property => SelectionTypeName,
        _ => throw new InvalidOperationException("Unsupported SelectionType")
    };
}