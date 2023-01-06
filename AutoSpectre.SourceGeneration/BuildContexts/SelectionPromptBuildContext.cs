using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.BuildContexts;

public class SelectionPromptBuildContext : PromptBuildContext
{
    public string Title { get; }
    public ITypeSymbol TypeSymbol { get; }
    public bool Nullable { get; }
    public string SelectionTypeName { get; set; }
    public SelectionPromptSelectionType SelectionType { get; }

    public SelectionPromptBuildContext(string title, ITypeSymbol typeSymbol, bool nullable, string selectionTypeName, SelectionPromptSelectionType selectionType)
    {
        Title = title;
        TypeSymbol = typeSymbol;
        Nullable = nullable;
        SelectionTypeName = selectionTypeName ?? throw new ArgumentNullException(nameof(selectionTypeName));
        SelectionType = selectionType;
    }

    public override string GenerateOutput(string destination)
    {
        var type = TypeSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().ToString() ?? TypeSymbol.ToDisplayString();

        return $"""
{destination} = AnsiConsole.Prompt(
new SelectionPrompt<{type}>()
.Title("{Title}")
.PageSize(10) 
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