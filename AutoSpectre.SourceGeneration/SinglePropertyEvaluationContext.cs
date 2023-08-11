using System;
using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoSpectre.SourceGeneration;

public class SinglePropertyEvaluationContext
{
    private Lazy<PropertyDeclarationSyntax?> _propertySyntaxLazy;
    
    public SinglePropertyEvaluationContext(IPropertySymbol property, bool isNullable, ITypeSymbol type, bool isEnumerable, ITypeSymbol underlyingType)
    {
        Property = property;
        IsNullable = isNullable;
        Type = type;
        IsEnumerable = isEnumerable;
        UnderlyingType = underlyingType;
        _propertySyntaxLazy = new Lazy<PropertyDeclarationSyntax?>(() =>
            Property.DeclaringSyntaxReferences[0].GetSyntax() as PropertyDeclarationSyntax);
    }

    public IPropertySymbol Property { get; }
    
    
    public bool IsNullable { get; }
    public ITypeSymbol Type { get; }
    public bool IsEnumerable { get; }
    public ITypeSymbol? UnderlyingType { get; }
    
    public ConfirmedConverter? ConfirmedConverter { get; set; }
    
    public ConfirmedValidator? ConfirmedValidator { get; set; }
    public ConfirmedCondition? ConfirmedCondition { get; set; }
    
    public ConfirmedDefaultValue? ConfirmedDefaultValue { get; set; }
    public PropertyDeclarationSyntax? PropertySyntax => _propertySyntaxLazy.Value;
    public string? PromptStyle { get; set; }
    public int? PageSize { get; set; }
    public bool? WrapAround { get; set; }

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