using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.Extensions
{
    public static class Extensions
    {
        internal static bool IsAttributeMatch(this string? source, string nameWithoutAttribute)
        {
            if (source == null)
                return false;

            return source.Equals(nameWithoutAttribute) || source.Equals($"{nameWithoutAttribute}Attribute");
        }

        public static T? GetValue<T>(this AttributeData attributeData, string name)
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

            return default;
        }

        public static IEnumerable<IPropertySymbol> GetPropertiesWithSetter(this INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(x => x.SetMethod != null);
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

        public static bool In<T>(this T source, params T[] candidates)
        {
            if (candidates == null) throw new ArgumentNullException(nameof(candidates));
            return candidates.Contains(source);
        }

        public static bool IsIn<TDest, TSource>(this TDest source, IEnumerable<TSource> items, Func<TSource, TDest, bool> predicate)
        {
            return items.Any(x => predicate(x,source));
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
                foreach (var namedTypeSymbol in namedType.AllInterfaces.Distinct<INamedTypeSymbol>(SymbolEqualityComparer.IncludeNullability))
                {
                   yield return namedTypeSymbol;
                }
            }
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
            if (typeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T && typeSymbol is INamedTypeSymbol { IsGenericType: true} namedType)
            {
                if (namedType.TypeArguments.FirstOrDefault() is {} argument)
                {
                    return (true, argument);
                }
            }

            if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated )
            {
                return (true, typeSymbol);
            }

            return (false, typeSymbol);
        }
    }
}