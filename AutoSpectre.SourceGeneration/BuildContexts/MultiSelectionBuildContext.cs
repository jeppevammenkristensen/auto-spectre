﻿using System;
using System.Linq;
using System.Text;
using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.BuildContexts;



public class MultiSelectionBuildContext : PromptBuildContext
{
    private readonly Types _types;
    private readonly Compilation _compilation;

    public delegate void ConverterDelegate(StringBuilder builder, string prompt);

    private Lazy<INamedTypeSymbol?> _genericList;

    private static INamedTypeSymbol ListFactory()
    {
        throw new NotImplementedException();
    }


    public MultiSelectionBuildContext(string title, ITypeSymbol typeSymbol, ITypeSymbol underlyingSymbol, bool nullable, string selectionTypeName, SelectionPromptSelectionType selectionType, Types types)
    {
        _types = types;
        Title = title;
        TypeSymbol = typeSymbol;
        UnderlyingSymbol = underlyingSymbol;
        Nullable = nullable;
        SelectionTypeName = selectionTypeName;
        SelectionType = selectionType;
    }

    public string Title { get; }
    public ITypeSymbol TypeSymbol { get; }
    public ITypeSymbol UnderlyingSymbol { get; }
    public bool Nullable { get; }

    public string SelectionTypeName { get; set; }
    public SelectionPromptSelectionType SelectionType { get; }
    


    public override string GenerateOutput(string destination)
    {
        var type = UnderlyingSymbol.GetTypePresentation();

        var conversion = NeedsConversion(TypeSymbol);

        var prompt = $"""
AnsiConsole.Prompt(
new MultiSelectionPrompt<{type}>()
.Title("{Title}")
{GenerateRequired()}
.PageSize(10) 
.AddChoices(destination.{GetSelector()}.ToArray()))
""";
        var builder = new StringBuilder();
        builder.Append($"{destination} = ");
        if (conversion is { })
            conversion(builder, prompt);
        else
        {
            builder.Append(prompt);
        }

        return builder.ToString();

    }

    private ConverterDelegate? NeedsConversion(ITypeSymbol typeSymbol)
    {
        if (typeSymbol.TypeKind is TypeKind.Array)
            return ToArray;

        // Match any of the below interfaces as they can all have a list assigned to them
        if (typeSymbol.IsOriginalSpecialTypeOf(
                SpecialType.System_Collections_Generic_ICollection_T,
                SpecialType.System_Collections_Generic_IEnumerable_T,
                SpecialType.System_Collections_Generic_IList_T,
                SpecialType.System_Collections_Generic_IReadOnlyList_T,
                SpecialType.System_Collections_Generic_IReadOnlyCollection_T))
            return null;
        // Check if the symbol is a List of T
        if (typeSymbol.OriginalDefinition.Equals(_types.ListGeneric, SymbolEqualityComparer.Default))
            return null;

        // May be a$$ biter. If the class is in the immutable namespace, we assume we can To{immutablenameofclass}

        if (typeSymbol.ContainingNamespace.ToDisplayString() == "System.Collections.Immutable" &&
            typeSymbol.Name.StartsWith("Immutable"))
        {
            return Immutable(typeSymbol);
        }

        // Check this is inherited from List
        if (typeSymbol.AllBaseTypes()
            .Any(x => x.OriginalDefinition.Equals(_types.ListGeneric, SymbolEqualityComparer.Default)))
            return null;

        if (typeSymbol.IsIn(new [] { _types.Collection, _types.HashSet}, (x,y) => x is {} && y.IsOriginalOfType(x)))
            return Wrappable(typeSymbol);

        return null;



    }

    private ConverterDelegate Immutable(ITypeSymbol typeSymbol)
    {
        return (builder, prompt) => builder.Append($"{prompt}.To{typeSymbol.Name}()");
    }

    private void ToArray(StringBuilder builder, string prompt)
    {
        builder.Append($"{prompt}.ToArray()");
    }

    private ConverterDelegate Wrappable(ITypeSymbol type)
    {
        return (stringBuilder, prompt) => stringBuilder.Append($"""new {type}({prompt})""");
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