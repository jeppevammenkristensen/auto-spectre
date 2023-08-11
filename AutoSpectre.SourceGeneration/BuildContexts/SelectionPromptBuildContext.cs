﻿using System;
using System.Linq;
using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.BuildContexts;

internal class SelectionPromptBuildContext : PromptBuilderContextWithPropertyContext
{
    public string Title { get; }
    public ITypeSymbol TypeSymbol { get; }
    public bool Nullable { get; }
    public string SelectionTypeName { get; set; }
    public SelectionPromptSelectionType SelectionType { get; }


    public SelectionPromptBuildContext(string title, SinglePropertyEvaluationContext context, string selectionTypeName, SelectionPromptSelectionType selectionType) : base(context)
    {
        Title = title;
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
            .AddChoices(destination.{GetSelector()}.ToArray()))
        """;
    }
    
    
    private string GenerateWrapAround()
    {
        if (Context.WrapAround is { })
        {
            return $"""".WrapAround({(Context.WrapAround.Value ? "true" : "false")})"""";
        }

        return string.Empty;
    }

    private string GeneratePageSize()
    {
        return $""".PageSize({(Context.PageSize == null ? "10" : Context.PageSize.ToString())})""";
    }
    

    private string GetSelector() => SelectionType switch
    {
        SelectionPromptSelectionType.Method => $"{SelectionTypeName}()",
        SelectionPromptSelectionType.Property => SelectionTypeName,
        _ => throw new InvalidOperationException("Unsupported SelectionType")
    };
}