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

        public  static T GetValue<T>(this AttributeData? attributeData, string property, int index)
        {
            var typeConstantKeyValuePair = attributeData?.NamedArguments.FirstOrDefault(x => x.Key == property);
            if (typeConstantKeyValuePair?.Key == property)
            {
                return (T)typeConstantKeyValuePair.Value.Value.Value;
            }

            if (attributeData?.ConstructorArguments.Length > index)
            {
                return (T)attributeData.ConstructorArguments[index].Value;
            }

            return default;
        }
    }
}