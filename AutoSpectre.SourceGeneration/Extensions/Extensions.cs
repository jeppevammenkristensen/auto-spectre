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

        public  static T? GetValue<T>(this AttributeData attributeData, string name)
        {
            if (attributeData == null) throw new ArgumentNullException(nameof(attributeData));

            var named = attributeData.NamedArguments.Select(x => new {  x.Key, Value = x.Value}).FirstOrDefault(x => x.Key == name);
            if (named != null)
            {
                return named.Value switch
                {
                    { Value: not {}} => default(T),
                    {Kind: TypedConstantKind.Enum} x => (T)Enum.ToObject(typeof(T), x.Value),
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
                    x.OriginalDefinition?.SpecialType == SpecialType.System_Collections_IEnumerable);

                foreach (var namedTypeSymbol in candidates)
                {
                    if (namedTypeSymbol.TypeArguments.FirstOrDefault() is { } result)
                        return (true, result);
                }
            }

            return (false,default!);
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