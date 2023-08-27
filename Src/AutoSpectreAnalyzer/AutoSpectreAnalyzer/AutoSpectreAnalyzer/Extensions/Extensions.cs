using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace AutoSpectreAnalyzer.Extensions;

public static class Extensions
{
    public static INamedTypeSymbol? GetAskAttribute(this Compilation compilation)
    {
        return compilation.GetTypeByMetadataName("AutoSpectre.AskAttribute");
    }

    public static INamedTypeSymbol? GetAutoSpectreFormAttribute(this Compilation compilation)
    {
        return compilation.GetTypeByMetadataName("AutoSpectre.AutoSpectreForm");
    }

    public static bool HasProperty(this INamedTypeSymbol symbol, string propertyName)
    {
        return symbol.GetMembers().OfType<IPropertySymbol>().Any(x => x.Name == propertyName);
    }

    public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attribute)
    {
        return symbol.GetAttributes().Any(x => x.AttributeClass?.Equals(attribute, SymbolEqualityComparer.Default) ?? false);
    }


    public static AttributeData? GetAttribute(this ISymbol symbol, INamedTypeSymbol attribute)
    {
        return symbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.Equals(attribute, SymbolEqualityComparer.Default) ?? false);
    }

    public static IEnumerable<IPropertySymbol> GetProperties(this INamedTypeSymbol source)
    {
        return source.GetMembers().OfType<IPropertySymbol>();
    }

    public static (bool isEnumerable, ITypeSymbol underlyingType) IsEnumerableOfTypeButNotString(
        this ITypeSymbol typeSymbol)
    {
        if (typeSymbol == null) throw new ArgumentNullException(nameof(typeSymbol));
        if (typeSymbol.SpecialType == SpecialType.System_String)
            return (false, default!);

        return IsEnumerableOfType(typeSymbol);
    }

    public static (bool isEnumerable, ITypeSymbol underlyingType) IsEnumerableOfType(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is IArrayTypeSymbol arrayType)
        {
            return (true, arrayType.ElementType);
        }

        if (typeSymbol is INamedTypeSymbol namedType)
        {
            if (namedType.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
                return (true, namedType.TypeArguments.First());

            var candidates = namedType.AllInterfaces.Where(x =>
                x.OriginalDefinition?.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T);

            foreach (var namedTypeSymbol in candidates)
            {
                if (namedTypeSymbol.TypeArguments.FirstOrDefault() is { } result)
                    return (true, result);
            }
        }

        return (false, default!);
    }
}