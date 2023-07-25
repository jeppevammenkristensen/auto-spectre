using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using System.Linq;
using AutoSpectre.SourceGeneration.Extensions;
using static AutoSpectre.SourceGeneration.BuildContexts.MultiSelectionBuildContext;

namespace AutoSpectre.SourceGeneration.BuildContexts;

public class MultiAddBuildContext : PromptBuildContext
{
    public override bool DeclaresVariable => true;

    private readonly ITypeSymbol _type;
    private readonly ITypeSymbol _underlyingType;
    private readonly LazyTypes _lazyTypes;
    private readonly PromptBuildContext _buildContext;

    private readonly SinglePropertyEvaluationContext? _childEvaluationContext;

    public MultiAddBuildContext(ITypeSymbol type, ITypeSymbol underlyingType,LazyTypes lazyTypes, PromptBuildContext buildContext)
    {
        _type = type;
        _underlyingType = underlyingType;
        _lazyTypes = lazyTypes;
        _buildContext = buildContext;
        if (buildContext is PromptBuilderContextWithPropertyContext withContext)
        {
            _childEvaluationContext = withContext.Context;
        }
    }

    public override string GenerateOutput(string destination)
    {
        StringBuilder builder = new();
        builder.AppendLine($"// Prompt for values for {destination}");
        builder.AppendLine("{");
        builder.AppendLine(PromptPart());
        builder.AppendLine($"{destination} = result;");
        builder.AppendLine("}");
        return builder.ToString();
    }

    public override string PromptPart(string? variable = null)
    {
        var type = _underlyingType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        StringBuilder builder = new();

        builder.AppendLine($$"""            
            List<{{type}}> items = new List<{{type}}>();
                bool continuePrompting = true;

                do 
                {
                    {{ GenerateAssignment() }}
                    continuePrompting = AnsiConsole.Confirm("Add more items?");
                    
                } while (continuePrompting);      
            """);

        builder.Append($"{_type.GetTypePresentation()} result =");

        if (NeedsConversion(_type) is { } converter)
        {
            converter(builder, "items");
        }
        else
        {
            builder.Append("items");
        }

        builder.AppendLine(";");
        return builder.ToString();
    }

    private string GenerateAssignment()
    {
        if (_buildContext.DeclaresVariable)
        {
            if (_childEvaluationContext?.ConfirmedValidator is {SingleValidation: false} validator)
            {
                return $$"""
                    bool valid = false;

                    while (!valid) {

                        {{_buildContext.PromptPart("newItem")}};
                        var validationResult = destination.{{validator.Name}}(items,newItem);
                        if (validationResult is {} error)
                        {
                            AnsiConsole.MarkupLine($"[red]{error}[/]");
                            valid = false;
                        }
                        else 
                        {
                            valid = true;
                            items.Add(newItem);
                        }
                    }
                    """;
            }
            else
            {
                return $$"""
            {
                {{_buildContext.PromptPart("newItem")}}
                items.Add(newItem);
            }
            """;
            }
            
        }
        else
        {
            if (_childEvaluationContext?.ConfirmedValidator is {SingleValidation: false} validator)
            {
                return $$"""
                    bool valid = false;

                    while (!valid) {

                        var item = {{_buildContext.PromptPart()}};
                        var validationResult = destination.{{validator.Name}}(items,item);
                        if (validationResult is {} error)
                        {
                            AnsiConsole.MarkupLine($"[red]{error}[/]");
                            valid = false;
                        }
                        else 
                        {
                            valid = true;
                            items.Add(item);
                        }
                    }
                    """;
            }
            else
            {
                return $"""
                var item = {_buildContext.PromptPart()};
                items.Add(item);
                """;
            }

            
        }
        
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
            return Wrappable(typeSymbol);

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

    private ConversionDelegate Wrappable(ITypeSymbol type)
    {
        return (stringBuilder, prompt) => stringBuilder.Append($"""new {type}({prompt})""");
    }

    public override IEnumerable<string> CodeInitializing()
    {
        return this._buildContext.CodeInitializing();
    }
}