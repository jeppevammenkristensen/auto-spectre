using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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

        public  static T? GetValue<T>(this AttributeData? attributeData, string property, int index)
        {
            KeyValuePair<string, TypedConstant>? typeConstantKeyValuePair = attributeData?.NamedArguments.FirstOrDefault(x => x.Key == property);
            if (typeConstantKeyValuePair?.Key == property)
            {
                return (T?)typeConstantKeyValuePair.Value.Value.Value;
            }

            if (attributeData?.ConstructorArguments.Length > index)
            {
                return (T?)attributeData.ConstructorArguments[index].Value;
            }

            return default;
        }

        public static IEnumerable<IPropertySymbol> GetPropertiesWithSetter(this INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(x => x.SetMethod != null);
        }

        public static (bool isNullable, ITypeSymbol type) GetTypeWithNullableInformation(this ITypeSymbol typeSymbol)
        {
            if (typeSymbol.OriginalDefinition?.SpecialType == SpecialType.System_Nullable_T && typeSymbol is INamedTypeSymbol { IsGenericType: true} namedType)
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