using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration;

internal class SinglePropertyEvaluationContext
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
}