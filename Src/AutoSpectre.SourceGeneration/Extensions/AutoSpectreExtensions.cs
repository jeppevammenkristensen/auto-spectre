using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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

    /// <summary>
    /// Gets the safe text with quotes.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <returns>The input text with quotes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input text is null.</exception>
    public static string GetSafeTextWithQuotes(this string text)
    {
        if (text == null) throw new ArgumentNullException(nameof(text));

        return SymbolDisplay.FormatLiteral(text, true);
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