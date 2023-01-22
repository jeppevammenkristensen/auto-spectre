﻿using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Spectre.Console;

namespace AutoSpectre.SourceGeneration.BuildContexts;

public class TextPromptBuildContext : PromptBuildContext
{
    public string Title { get; }
    public ITypeSymbol TypeSymbol { get; }
    public bool Nullable { get; }

    public TextPromptBuildContext(string title, ITypeSymbol typeSymbol, bool nullable)
    {
        Title = title;
        TypeSymbol = typeSymbol;
        Nullable = nullable;
    }

    public override string GenerateOutput(string destination)
    {
        return $"{destination} = {PromptPart()};";
    }

    public override string PromptPart(string? variableName = null)
    {
        var syntax = TypeSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().ToString() ?? TypeSymbol.ToDisplayString();
        StringBuilder builder = new();
        builder.AppendLine("AnsiConsole.Prompt(");
        builder.AppendLine($"""new TextPrompt<{syntax}>("{Title}")""");
        if (Nullable)
        {
            builder.AppendLine(".AllowEmpty()");
        }

        builder.Append(")");
        return builder.ToString();
    }
}