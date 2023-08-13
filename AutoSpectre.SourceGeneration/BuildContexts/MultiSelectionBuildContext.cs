using System;
using System.Linq;
using System.Text;
using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.BuildContexts;

internal class MultiSelectionBuildContext : SelectionBaseBuildContext
{
    private readonly LazyTypes _lazyTypes;

    public delegate void ConversionDelegate(StringBuilder builder, string prompt);

    public MultiSelectionBuildContext(string title, SinglePropertyEvaluationContext context, string selectionTypeName, SelectionPromptSelectionType selectionType, LazyTypes lazyTypes) : base(title, context)
    {
        _lazyTypes = lazyTypes;
        TypeSymbol = context.Type;
        UnderlyingSymbol = context.UnderlyingType;
        Nullable = context.IsNullable;
        SelectionTypeName = selectionTypeName;
        SelectionType = selectionType;
    }
    public ITypeSymbol TypeSymbol { get; }
    public ITypeSymbol? UnderlyingSymbol { get; }
    public bool Nullable { get; }

    public string SelectionTypeName { get; set; }
    public SelectionPromptSelectionType SelectionType { get; }
    


    public override string GenerateOutput(string destination)
    {
        StringBuilder builder = new ();
        builder.Append($"{destination} = ");
        builder.Append(PromptPart());

        return builder.ToString();
    }

    public override string PromptPart(string? variableName = null)
    {
        var type = UnderlyingSymbol.ToDisplayString();

        var conversion = NeedsConversion(TypeSymbol);

        var prompt = $"""
AnsiConsole.Prompt(
new MultiSelectionPrompt<{type}>()
.Title("{Title}"){GenerateConverter()}
{GenerateRequired()}
{GeneratePageSize()}
{GenerateWrapAround()}
{GenerateMoreChoicesText()}
.AddChoices(destination.{GetSelector()}.ToArray()))
""";
        var builder = new StringBuilder(150);

        if (conversion is { })
            conversion(builder, prompt);
        else
        {
            builder.Append(prompt);
        }

        builder.Append(";");
        return builder.ToString();
    }

    

    private ConversionDelegate? NeedsConversion(ITypeSymbol typeSymbol)
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
        if (typeSymbol.OriginalDefinition.Equals(_lazyTypes.ListGeneric, SymbolEqualityComparer.Default))
            return null;

        // May be a$$ biter. If the class is in the immutable namespace, we assume we can To{immutablenameofclass}

        if (typeSymbol.ContainingNamespace.ToDisplayString() == "System.Collections.Immutable" &&
            typeSymbol.Name.StartsWith("Immutable"))
        {
            return Immutable(typeSymbol);
        }

        // Check this is inherited from List
        if (typeSymbol.AllBaseTypes()
            .Any(x => x.OriginalDefinition.Equals(_lazyTypes.ListGeneric, SymbolEqualityComparer.Default)))
            return null;

        if (typeSymbol.IsIn(new [] { _lazyTypes.Collection, _lazyTypes.HashSet}, (x,y) => x is {} && y.IsOriginalOfType(x)))
            return Wrapable(typeSymbol);

        return null;



    }

    private ConversionDelegate Immutable(ITypeSymbol typeSymbol)
    {
        return (builder, prompt) => builder.Append($"{prompt}.To{typeSymbol.Name}()");
    }

    private void ToArray(StringBuilder builder, string prompt)
    {
        builder.Append($"{prompt}.ToArray()");
    }

    private ConversionDelegate Wrapable(ITypeSymbol type)
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