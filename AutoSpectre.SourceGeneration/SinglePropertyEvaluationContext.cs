using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration;

public class SinglePropertyEvaluationContext
{
    public SinglePropertyEvaluationContext(IPropertySymbol property, bool isNullable, ITypeSymbol type, bool isEnumerable, ITypeSymbol underlyingType)
    {
        Property = property;
        IsNullable = isNullable;
        Type = type;
        IsEnumerable = isEnumerable;
        UnderlyingType = underlyingType;
    }

    public IPropertySymbol Property { get; }
    public bool IsNullable { get; }
    public ITypeSymbol Type { get; }
    public bool IsEnumerable { get; }
    public ITypeSymbol? UnderlyingType { get; }
    
    public ConfirmedConverter? ConfirmedConverter { get; set; }
    
    public ConfirmedValidator? ConfirmedValidator { get; set; }
    public ConfirmedCondition? ConfirmedCondition { get; set; }

    public static SinglePropertyEvaluationContext GenerateFromPropertySymbol(IPropertySymbol property)
    {
        var (nullable, originalType) = property.Type.GetTypeWithNullableInformation();
        var (enumerable, underlying) = property.Type.IsEnumerableOfTypeButNotString();

        var propertyEvaluationContext =
            new SinglePropertyEvaluationContext(property: property, isNullable: nullable, type: originalType,
                isEnumerable: enumerable, underlyingType: underlying);
        return propertyEvaluationContext;
    }
}