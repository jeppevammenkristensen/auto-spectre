using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.BuildContexts;

public class MultiSelectionBuildContext : PromptBuildContext
{
    public MultiSelectionBuildContext(string title, ITypeSymbol typeSymbol, bool nullable, string selectionTypeName, SelectionPromptSelectionType selectionType)
    {
        Title = title;
        TypeSymbol = typeSymbol;
        Nullable = nullable;
        SelectionTypeName = selectionTypeName;
        SelectionType = selectionType;
    }

    public string Title { get; }
    public ITypeSymbol TypeSymbol { get; }
    public bool Nullable { get; }

    public string SelectionTypeName { get; set; }
    public SelectionPromptSelectionType SelectionType { get; }


    public override string GenerateOutput()
    {
        var type = TypeSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().ToString() ?? TypeSymbol.ToDisplayString();

        return $"""
AnsiConsole.Prompt(
new MultiSelectionPrompt<{type}>()
.Title("{Title}")
{GenerateRequired()}
.PageSize(10) 
.AddChoices(destination.{GetSelector()}.ToArray()))
""";
    }

    private string GenerateRequired()
    {
        return Nullable ? ".NotRequired()" : string.Empty;
    }

    private string GetSelector() => SelectionType switch
    {
        SelectionPromptSelectionType.Method => $"{SelectionTypeName}()",
        SelectionPromptSelectionType.Property => SelectionTypeName,
        _ => throw new InvalidOperationException("Unsupported SelectionType")
    };
}