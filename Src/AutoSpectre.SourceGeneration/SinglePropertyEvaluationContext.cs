using System;
using System.Diagnostics;
using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoSpectre.SourceGeneration;

public class SingleMethodEvaluationContext : IConditionContext
{
    public IMethodSymbol Method { get; }
    public bool ReturnTypeIsTask { get; }
    public bool HasAnsiConsoleParameter { get; }

    private Lazy<MethodDeclarationSyntax> _methodSyntaxLazy;

    public SingleMethodEvaluationContext(IMethodSymbol method, bool returnTypeIsTask, bool hasAnsiConsoleParameter)
    {
        Method = method;
        ReturnTypeIsTask = returnTypeIsTask;
        HasAnsiConsoleParameter = hasAnsiConsoleParameter;
        _methodSyntaxLazy = new Lazy<MethodDeclarationSyntax>(() =>
            Method.DeclaringSyntaxReferences[0].GetSyntax() as MethodDeclarationSyntax ?? throw new InvalidOperationException());
    }

    public ConfirmedStatusWrap? ConfirmedStatus { get; set; }
    public MethodDeclarationSyntax MethodSyntax => _methodSyntaxLazy.Value;
    public string? SpinnerStyle { get; set; }
    public string? SpinnerKnownType { get; set; }
    string IConditionContext.Name => Method.Name;
    ISymbol IConditionContext.Symbol => Method;
    public ConfirmedCondition? ConfirmedCondition { get; set; }
}

public class ConfirmedStatusWrap
{
    public ConfirmedStatusWrap(string statusText)
    {
        StatusText = statusText;
    }
    public string StatusText { get; }
    
}

public interface IConditionContext
{
    public string Name { get; }
    public ISymbol Symbol { get; }

    public ConfirmedCondition? ConfirmedCondition { get; set; }
}

public class SingleFormEvaluationContext
{
    public ConfirmedCulture? ConfirmedCulture { get; set; }
}

public class ConfirmedCulture
{
    public string Culture { get; }

    public ConfirmedCulture(string culture)
    {
        Culture = culture;
    }
}

public class SinglePropertyEvaluationContext : IConditionContext
{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    public static SinglePropertyEvaluationContext Empty = new SinglePropertyEvaluationContext(default(IPropertySymbol),
        false, default(ITypeSymbol), false, default(ITypeSymbol));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    
    private Lazy<PropertyDeclarationSyntax?> _propertySyntaxLazy;

    public bool RequiresAsync => ConfirmedNamedTypeSource?.IsAsync ?? false;
    
    public SinglePropertyEvaluationContext(IPropertySymbol property, bool isNullable, ITypeSymbol type, bool isEnumerable, ITypeSymbol? underlyingType)
    {
        Property = property;
        IsNullable = isNullable;
        Type = type;
        IsEnumerable = isEnumerable;
        if (underlyingType is { })
        {
            var (underlyingCanBeNull, underlying) = underlyingType.GetTypeWithNullableInformation();
            UnderlyingType = underlying;
            UnderlyingTypeIsNullable = underlyingCanBeNull;
        }
        
        _propertySyntaxLazy = new Lazy<PropertyDeclarationSyntax?>(() =>
            Property.DeclaringSyntaxReferences[0].GetSyntax() as PropertyDeclarationSyntax);
    }

    public bool UnderlyingTypeIsNullable { get;  }

    public IPropertySymbol Property { get; }

    string IConditionContext.Name => Property.Name;
    ISymbol IConditionContext.Symbol => Property;
    
    
    public bool IsNullable { get; }
    public ITypeSymbol Type { get; }
    public bool IsEnumerable { get; }
    public ITypeSymbol? UnderlyingType { get; }

    public ConfirmedSelectionSource ConfirmedSelectionSource { get; set; }

    public ConfirmedNamedTypeSource? ConfirmedNamedTypeSource { get; set; }
    
    public ConfirmedConverter? ConfirmedConverter { get; set; }
    
    public ConfirmedValidator? ConfirmedValidator { get; set; }
    public ConfirmedCondition? ConfirmedCondition { get; set; }
    
    public ConfirmedDefaultValue? ConfirmedDefaultValue { get; set; }
    public PropertyDeclarationSyntax? PropertySyntax => _propertySyntaxLazy.Value;
    public string? PromptStyle { get; set; }
    public int? PageSize { get; set; }
    public bool? WrapAround { get; set; }
    public string? MoreChoicesText { get; set; }
    public string? InstructionsText { get; set; }
    public string? HighlightStyle { get; set; } //public NamedTypedAnalysis? NamedTypeAnalysis { get; set; }
    

    public static SinglePropertyEvaluationContext GenerateFromPropertySymbol(IPropertySymbol property)
    {
        var (nullable, originalType) = property.Type.GetTypeWithNullableInformation();
        var (enumerable, underlying) = property.Type.IsEnumerableOfTypeButNotString();

        var propertyEvaluationContext =
            new SinglePropertyEvaluationContext(property: property, isNullable: nullable, type: originalType,
                isEnumerable: enumerable, underlyingType: underlying);
        return propertyEvaluationContext;
    }

    /// <summary>
    ///  Returns the Type is the type is not an enumerable or the underlying
    /// type is it is an enumerable
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public (bool nullable, ITypeSymbol type) GetSingleType() =>
        IsEnumerable
            ? (UnderlyingTypeIsNullable,
                UnderlyingType ?? throw new InvalidOperationException("This should not be null"))
            : (IsNullable, Type);

}