using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.Extensions;

public static class AutoSpectreExtensions
{
    internal static string GetSpectreFactoryClassName(this ITypeSymbol typeSymbol)
    {
        return $"{typeSymbol.Name}SpectreFactory";
    }

    internal static string GetSpectreFactoryInterfaceName(this ITypeSymbol typeSymbol)
    {
        return $"I{typeSymbol.GetSpectreFactoryClassName()}";
    }

    internal static string GetSpectreVariableName(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol.GetSpectreFactoryClassName() is { Length:> 0} name)
        {
            return string.Concat(name[0].ToString().ToUpper(), name.Substring(1));
        }

        throw new InvalidOperationException("Expected to get a name with at least 1 character");
    }

    public static string GetStaticOrInstance(this string variable, string targetType,bool isStatic)
    {
        return isStatic ? targetType : variable;
    }

    public static IMethodSymbol? FindConstructor(this INamedTypeSymbol source, LazyTypes types)
    {
        return source.Constructors
            .Where(x => x.DeclaredAccessibility is Accessibility.Public)
            .OrderBy(x => 
        {
            if (x.GetAttributes().Any(y => y.AttributeClass?.Equals(types.UsedConstructorAttribute, SymbolEqualityComparer.Default) == true))
            {
                return -1;
            }

            return x.Parameters.Length;
        }).FirstOrDefault();

    }
}