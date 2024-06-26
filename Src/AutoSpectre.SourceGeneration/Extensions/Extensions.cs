using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoSpectre.SourceGeneration.Extensions;

public static class Extensions
{
    public static bool EvaluateStyle(this string text)
    {
        Decoration? effectiveDecoration = null;
        object? effectiveForeground = null;
        object? effectiveBackground = null;
        string? effectiveLink = null;
        string error = null;

        var parts = text.Split(new[] {' '});
        var foreground = true;
        foreach (var part in parts)
        {
            if (part.Equals("default", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (part.Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                foreground = false;
                continue;
            }

            if (part.StartsWith("link=", StringComparison.OrdinalIgnoreCase))
            {
                if (effectiveLink != null)
                {
                    error = "A link has already been set.";
                    return false;
                }

                effectiveLink = part.Substring(5);
                continue;
            }
            else if (part.StartsWith("link", StringComparison.OrdinalIgnoreCase))
            {
                effectiveLink = "emptylink";
                continue;
            }

            var decoration = DecorationTable.GetDecoration(part);
            if (decoration != null)
            {
                effectiveDecoration ??= Decoration.None;

                effectiveDecoration |= decoration.Value;
            }
            else
            {
                var color = ColorTable.GetColor(part);
                if (color == null)
                {
                    if (part.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                    {
                        color = ParseHexColor(part, out error);
                        if (!string.IsNullOrWhiteSpace(error))
                        {
                            return false;
                        }
                    }
                    else if (part.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
                    {
                        color = ParseRgbColor(part, out error);
                        if (!string.IsNullOrWhiteSpace(error))
                        {
                            return false;
                        }
                    }
                    else if (int.TryParse(part, out var number))
                    {
                        if (number < 0)
                        {
                            error = $"Color number must be greater than or equal to 0 (was {number})";
                            return false;
                        }
                        else if (number > 255)
                        {
                            error = $"Color number must be less than or equal to 255 (was {number})";
                            return false;
                        }

                        color = Color.Aqua; // cheating
                    }
                    else
                    {
                        error = !foreground
                            ? $"Could not find color '{part}'."
                            : $"Could not find color or style '{part}'.";

                        return false;
                    }
                }

                if (foreground)
                {
                    if (effectiveForeground != null)
                    {
                        error = "A foreground color has already been set.";
                        return false;
                    }

                    effectiveForeground = color;
                }
                else
                {
                    if (effectiveBackground != null)
                    {
                        error = "A background color has already been set.";
                        return false;
                    }

                    effectiveBackground = color;
                }
            }
        }

       
        return true;
    }

    internal static string ReplaceExact(this string text, string oldValue, string? newValue)
    {
#if NETSTANDARD2_0
        return text.Replace(oldValue, newValue);
#else
        return text.Replace(oldValue, newValue, StringComparison.Ordinal);
#endif
    }

    private static Color? ParseHexColor(string hex, out string? error)
    {
        error = null;

        hex ??= string.Empty;
        hex = hex.ReplaceExact("#", string.Empty).Trim();

        try
        {
            if (!string.IsNullOrWhiteSpace(hex))
            {
                if (hex.Length == 6)
                {
                    return new Color(
                        (byte) Convert.ToUInt32(hex.Substring(0, 2), 16),
                        (byte) Convert.ToUInt32(hex.Substring(2, 2), 16),
                        (byte) Convert.ToUInt32(hex.Substring(4, 2), 16));
                }
                else if (hex.Length == 3)
                {
                    return new Color(
                        (byte) Convert.ToUInt32(new string(hex[0], 2), 16),
                        (byte) Convert.ToUInt32(new string(hex[1], 2), 16),
                        (byte) Convert.ToUInt32(new string(hex[2], 2), 16));
                }
            }
        }
        catch (Exception ex)
        {
            error = $"Invalid hex color '#{hex}'. {ex.Message}";
            return null;
        }

        error = $"Invalid hex color '#{hex}'.";
        return null;
    }

    private static Color? ParseRgbColor(string rgb, out string? error)
    {
        try
        {
            error = null;

            var normalized = rgb ?? string.Empty;
            if (normalized.Length >= 3)
            {
                // Trim parentheses
                normalized = normalized.Substring(3).Trim();

                if (normalized.StartsWith("(", StringComparison.OrdinalIgnoreCase) &&
                    normalized.EndsWith(")", StringComparison.OrdinalIgnoreCase))
                {
                    normalized = normalized.Trim('(').Trim(')');

                    var parts = normalized.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 3)
                    {
                        return new Color(
                            (byte) Convert.ToInt32(parts[0], CultureInfo.InvariantCulture),
                            (byte) Convert.ToInt32(parts[1], CultureInfo.InvariantCulture),
                            (byte) Convert.ToInt32(parts[2], CultureInfo.InvariantCulture));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            error = $"Invalid RGB color '{rgb}'. {ex.Message}";
            return null;
        }

        error = $"Invalid RGB color '{rgb}'.";
        return null;
    }

    internal static bool IsPublicInstance(this ISymbol source)
    {
        return source.IsPublic() && source.IsInstance();
    }
    
    internal static bool IsPublic(this ISymbol source)
    {
        return source.DeclaredAccessibility == Accessibility.Public;
    }
    
    internal static bool IsInternal(this ISymbol source)
    {
        return source.DeclaredAccessibility == Accessibility.Internal;
    }

    internal static bool IsInstance(this ISymbol source)
    {
        return !source.IsStatic;
    }

    /// <summary>
    /// this will return the type name including any parents if it's nested
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    internal static string FullName(this INamedTypeSymbol source)
    {
        var format = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters);
        return source.ToDisplayString(format);
    }
    
    
    /// <summary>
    /// Gets all members and members of base types that are not interfaces
    /// It will return all types of members. But will have a special focus on Properties
    /// and Methods. If the method of a base class is virtual and it matches the signature of an existing members
    /// in a derived class it will not be returned
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    internal static List<ISymbol> GetAllMembers(this INamedTypeSymbol source)
    {
        List<ISymbol> members = new(source.GetMembers());

        if (source.BaseType is {TypeKind: TypeKind.Class, SpecialType: SpecialType.None} baseType) 
        {
            foreach (var member in baseType.GetAllMembers())
            {
                if (member.IsAbstract)
                    continue;
                if (member.IsVirtual && members.Find(x => x.IsApproximateMemberMatch(member)) == null)
                    continue;
                members.Add(member);
            }
        }

        return members;
    }

    internal static bool IsApproximateMemberMatch(this ISymbol source, ISymbol other)
    {
        if (source.Kind == other.Kind && source.Name == other.Name)
        {
            if (source is IMethodSymbol methodSymbol && other is IMethodSymbol otherMethodSymbol)
            {
                if (methodSymbol.Parameters.Length != otherMethodSymbol.Parameters.Length)
                    return false;

                for (int i = 0; i < methodSymbol.Parameters.Length; i++)
                {
                    if (!methodSymbol.Parameters[i].Type.Equals(otherMethodSymbol.Parameters[i].Type, SymbolEqualityComparer.Default))
                        return false;
                }

                return true;
            }
            else if (source is IPropertySymbol propertySymbol && other is IPropertySymbol otherPropertySymbol)
            {
                return propertySymbol.Type.Equals(otherPropertySymbol.Type, SymbolEqualityComparer.Default);
            }
            else if (source is IFieldSymbol fieldSymbol && other is IFieldSymbol otherFieldSymbol)
            {
                return fieldSymbol.Type.Equals(otherFieldSymbol.Type, SymbolEqualityComparer.Default);
            }
        }

        // We currently only have interest in properties and symbols
        return false;
            
    }

    internal static bool IsAttributeMatch(this string? source, string nameWithoutAttribute)
    {
        if (source == null)
            return false;

        return source.Equals(nameWithoutAttribute) || source.Equals($"{nameWithoutAttribute}Attribute");
    }

    public static T? GetAttributePropertyValue<T>(this AttributeData attributeData, string name, T? valueIfNotPresent = default)
    {
        if (attributeData == null) throw new ArgumentNullException(nameof(attributeData));

        var named = attributeData.NamedArguments.Select(x => new {x.Key, Value = x.Value})
            .FirstOrDefault(x => x.Key == name);
        if (named != null)
        {
            return named.Value switch
            {
                {Value: not { }} => default(T),
                {Kind: TypedConstantKind.Enum} x => (T) Enum.ToObject(typeof(T), x.Value),
                _ => (T?) named.Value.Value
            };
        }

        //attributeData.ConstructorArguments.FirstOrDefault(x => x.())

        return valueIfNotPresent;
    }
    
    public static bool TryGetAttributePropertyValue<T>(this AttributeData attributeData, string name, out T value)
    {
        value = default(T);
        
        if (attributeData == null) throw new ArgumentNullException(nameof(attributeData));

        var named = attributeData.NamedArguments.Select(x => new {x.Key, Value = x.Value})
            .FirstOrDefault(x => x.Key == name);
        if (named != null)
        {
            var (valid, r) = named.Value switch
            {
                {Value: not { }} => (false,default),
                {Kind: TypedConstantKind.Enum} x => (true,(T) Enum.ToObject(typeof(T), x.Value)),
                _ => (true,(T) named.Value.Value)
            };

            if (valid)
                value = r;
            
            return valid;
        }

        //attributeData.ConstructorArguments.FirstOrDefault(x => x.())

        return false;
    }

    public static IEnumerable<IPropertySymbol> GetAllProperties(this INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.GetAllMembers().OfType<IPropertySymbol>();
    }

    public static IEnumerable<(IPropertySymbol? property,IMethodSymbol? method)> GetPropertiesWithSetterAndMethods(this INamedTypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetAllMembers())
        {
            if (member is IPropertySymbol { SetMethod: { } } property)
            {
                yield return (property, null);
            }
            else if (member is IMethodSymbol methodSymbol)
            {
                yield return (null, methodSymbol);
            }
        }
    }

    public static (bool isEnumerable, ITypeSymbol underlyingType) IsEnumerableOfTypeButNotString(
        this ITypeSymbol typeSymbol)
    {
        if (typeSymbol == null) throw new ArgumentNullException(nameof(typeSymbol));
        if (typeSymbol.SpecialType == SpecialType.System_String)
            return (false, default!);

        return IsEnumerableOfType(typeSymbol);
    }
    public static bool IsMethodReturningTask(this IMethodSymbol method, SemanticModel model)
    {
        var task = model.Compilation.GetTypeByMetadataName("System.Threading.Tasks");
        if (task is null)
            return false;

        if (method.ReturnType.Equals(task, SymbolEqualityComparer.Default))
        {
            return true;
        }

        return false;
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

    public static bool In<T>(this T source, params T[] candidates)
    {
        if (candidates == null) throw new ArgumentNullException(nameof(candidates));
        return candidates.Contains(source);
    }

    public static bool IsIn<TDest, TSource>(this TDest source, IEnumerable<TSource> items,
        Func<TSource, TDest, bool> predicate)
    {
        return items.Any(x => predicate(x, source));
    }

    public static bool IsOriginalSpecialTypeOf(this ITypeSymbol source, params SpecialType[] specialType)
    {
        return source.OriginalDefinition.SpecialType.In(specialType);
    }

    public static bool IsOriginalOfType(this ITypeSymbol source, ITypeSymbol? type)
    {
        if (type == null) return false;

        return source.OriginalDefinition.Equals(type, SymbolEqualityComparer.Default);
    }

    public static string GetTypePresentation(this ITypeSymbol type)
    {
        return type.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().ToString() ?? type.ToDisplayString();
    }

    public static IEnumerable<ITypeSymbol> TypeAndInterfaces(this ITypeSymbol source)
    {
        yield return source;

        if (source is INamedTypeSymbol namedType)
        {
            foreach (var namedTypeSymbol in namedType.AllInterfaces.Distinct<INamedTypeSymbol>(SymbolEqualityComparer
                         .IncludeNullability))
            {
                yield return namedTypeSymbol;
            }
        }
    }

    public static bool IsDecoratedWithAttribute(this ISymbol symbol, INamedTypeSymbol attribute)
    {
        return symbol.IsDecoratedWithAnyAttribute(attribute);
    }

    public static IEnumerable<TSymbol> FilterDecoratedWithAnyAttribute<TSymbol>(this IEnumerable<TSymbol> symbols,
        params INamedTypeSymbol[] attributes) where TSymbol : ISymbol
    {
        var hashset = new HashSet<INamedTypeSymbol>(attributes, SymbolEqualityComparer.Default);
        return symbols.Where(x =>
            x.GetAttributes().Any(y => y.AttributeClass is { } attributeClass && hashset.Contains(attributeClass)));
    }

    public static bool IsDecoratedWithAnyAttribute(this ISymbol symbol, params INamedTypeSymbol[] attributes)
    {
        var hashset = new HashSet<INamedTypeSymbol>(attributes, SymbolEqualityComparer.Default);
        return symbol.GetAttributes().Any(x => x.AttributeClass is { } attribute && hashset.Contains(attribute));
    }
    public static IEnumerable<ITypeSymbol> AllBaseTypes(this ITypeSymbol source)
    {
        ITypeSymbol? current = source;

        yield return source;

        while (current is INamedTypeSymbol currentTypeSymbol)
        {
            if (currentTypeSymbol.BaseType is { } baseType)
            {
                yield return baseType;
                current = baseType;
            }
            else
            {
                current = null;
            }
        }
    }


    public static (bool isNullable, ITypeSymbol type) GetTypeWithNullableInformation(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
            typeSymbol is INamedTypeSymbol {IsGenericType: true} namedType)
        {
            if (namedType.TypeArguments.FirstOrDefault() is { } argument)
            {
                return (true, argument);
            }
        }

        if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return (true, typeSymbol);
        }

        return (false, typeSymbol);
    }
}

