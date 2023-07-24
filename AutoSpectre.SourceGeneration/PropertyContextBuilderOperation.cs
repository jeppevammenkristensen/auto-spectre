using System;
using System.Collections.Generic;
using System.Linq;
using AutoSpectre.SourceGeneration.BuildContexts;
using AutoSpectre.SourceGeneration.Extensions;
using AutoSpectre.SourceGeneration.Models;
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
}

internal class ConfirmedConverter
{
    public string Converter { get; }

    public ConfirmedConverter(string converter)
    {
        Converter = converter;
    }
}

internal class PropertyContextBuilderOperation
{
    public GeneratorAttributeSyntaxContext SyntaxContext { get; }
    public IReadOnlyList<PropertyWithAskAttributeData> PropertyCandidates { get; }
    public INamedTypeSymbol TargetType { get; }
    public SourceProductionContext ProductionContext { get; }

    public LazyTypes Types { get;  }

    internal PropertyContextBuilderOperation(
        GeneratorAttributeSyntaxContext syntaxContext, 
        IReadOnlyList<PropertyWithAskAttributeData> propertyCandidates,
        INamedTypeSymbol targetType, SourceProductionContext productionContext)
    {
        SyntaxContext = syntaxContext;
        PropertyCandidates = propertyCandidates;
        TargetType = targetType;
        ProductionContext = productionContext;
        Types = new(SyntaxContext.SemanticModel.Compilation);
    }

    public static List<PropertyContext> GetPropertyContexts(
        GeneratorAttributeSyntaxContext syntaxContext, 
        IReadOnlyList<PropertyWithAskAttributeData> candidates,
        INamedTypeSymbol targetNamedType, 
        SourceProductionContext productionContext)
    {
        PropertyContextBuilderOperation operation = new(syntaxContext, candidates, targetNamedType, productionContext);
        return operation.GetPropertyContexts();
    }

    public List<PropertyContext> GetPropertyContexts()
    {
        var types = new LazyTypes(SyntaxContext.SemanticModel.Compilation);

        List<PropertyContext> propertyContexts = new();

        foreach (var (property, attributeData) in PropertyCandidates)
        {
            var (nullable, originalType) = property.Type.GetTypeWithNullableInformation();
            var (enumerable, underlying) = property.Type.IsEnumerableOfTypeButNotString();
            
            var  propertyEvaluationContext =
                new SinglePropertyEvaluationContext(property: property,isNullable: nullable, type: originalType, isEnumerable: enumerable, underlyingType: underlying);

            if (attributeData.AskType == AskTypeCopy.Normal)
            {
                if (!propertyEvaluationContext.IsEnumerable)
                {
                    if (GetNormalPromptBuildContext(attributeData.Title, propertyEvaluationContext) is
                        { } promptBuildContext)
                    {
                        propertyContexts.Add(new(property.Name, property,
                            promptBuildContext));
                    }
                }
                else
                {
                    if (GetNormalPromptBuildContext(attributeData.Title, propertyEvaluationContext) is
                        { } promptBuildContext)
                    {
                        propertyContexts.Add(new(property.Name, property, new MultiAddBuildContext(propertyEvaluationContext.Type, propertyEvaluationContext.UnderlyingType, types, promptBuildContext)));
                    }
                }
            }

            if (attributeData.AskType == AskTypeCopy.Selection)
            {
                EvaluateSelectionConverter(attributeData, propertyEvaluationContext);
                
                if (attributeData.SelectionSource is { })
                {
                    var match = TargetType
                        .GetMembers()
                        .Where(x => x.Name == attributeData.SelectionSource)
                        .FirstOrDefault(x => x is IMethodSymbol
                        {
                            Parameters.Length: 0
                        } or IPropertySymbol {GetMethod: { }});

                    if (match is { })
                    {
                        SelectionPromptSelectionType selectionType = match switch
                        {
                            IMethodSymbol => SelectionPromptSelectionType.Method,
                            IPropertySymbol => SelectionPromptSelectionType.Property,
                            _ => throw new NotSupportedException(),
                        };
                        if (!propertyEvaluationContext.IsEnumerable)
                        {
                            propertyContexts.Add(new(property.Name, property,
                                new SelectionPromptBuildContext(attributeData.Title, propertyEvaluationContext,
                                    attributeData.SelectionSource, selectionType)));
                        }
                        else
                        {
                            propertyContexts.Add(new(property.Name, property,
                                new MultiSelectionBuildContext(title: attributeData.Title,
                                    propertyEvaluationContext,
                                    selectionTypeName: attributeData.SelectionSource,
                                    selectionType: selectionType, types)));
                        }
                    }
                    else
                    {
                        ProductionContext.ReportDiagnostic(Diagnostic.Create(
                            new("AutoSpectre_JJK0005",
                                "Not a valid selection source",
                                $"The selectionsource {attributeData.SelectionSource} was not found on type",
                                "General", DiagnosticSeverity.Warning, true),
                            property.Locations.FirstOrDefault()));
                    }
                }
            }
        }

        return propertyContexts;
    }

    /// <summary>
    /// Evalutes the Converter set on the attributeData. If it's correct a valid converter is set on the context.
    /// if it is set but not valid a warning is reported.
    /// </summary>
    /// <param name="attributeData"></param>
    /// <param name="context"></param>
    private void EvaluateSelectionConverter(TranslatedAskAttributeData attributeData, SinglePropertyEvaluationContext context)
    {
        bool guessed = attributeData.Converter == null;
        var converterName = attributeData.Converter ?? $"{context.Property.Name}Converter";
        
        bool ConverterMethodOrProperty(ISymbol symbol)
        {
            if (symbol is IMethodSymbol method)
            {
                if (method.Parameters.FirstOrDefault() is { } parameter)
                {
                    if (SymbolEqualityComparer.Default.Equals(parameter.Type,context.UnderlyingType ?? context.Type))
                    {
                        if (method.ReturnType.SpecialType == SpecialType.System_String)
                        {
                            return true;
                        }   
                    }
                }
            }

            return false;
        }

        var match = TargetType
            .GetMembers()
            .Where(x => x.Name == converterName)
            .FirstOrDefault(ConverterMethodOrProperty);

        if (match is { })
        {
            context.ConfirmedConverter = new ConfirmedConverter(converterName);
        }
        else if (!guessed)
        {
            ProductionContext.ReportDiagnostic(Diagnostic.Create(
                new("AutoSpectre_JJK0008", $"Converter {attributeData.Converter} should be a method taking a {context.UnderlyingType} as input and return string on the class",
                    $"Could not find a correct method to match {attributeData.Converter} supported", "General", DiagnosticSeverity.Warning, true),
                context.Property.Locations.FirstOrDefault()));
        }
    }
    
    public PromptBuildContext? GetNormalPromptBuildContext(string title, SinglePropertyEvaluationContext evaluationContext)
    {
        var type = evaluationContext.IsEnumerable ? evaluationContext.UnderlyingType : evaluationContext.Type;
        
        if (type.SpecialType == SpecialType.System_Boolean)
        {
            return new ConfirmPromptBuildContext(title, type, evaluationContext.IsNullable);
        }
        else if (type.TypeKind == TypeKind.Enum)
        {
            return new EnumPromptBuildContext(title, type, evaluationContext.IsNullable);
        }
        else if (type.SpecialType == SpecialType.None)
        {
            if (type is INamedTypeSymbol namedType)
            {
                if (namedType.GetAttributes().FirstOrDefault(x =>
                        SymbolEqualityComparer.Default.Equals(x.AttributeClass,Types.AutoSpectreForm)) is { })
                {
                    return new ReuseExistingAutoSpectreFactoryPromptBuildContext(title, namedType, evaluationContext.IsNullable);
                }
                else
                {
                    ProductionContext.ReportDiagnostic(Diagnostic.Create(
                        new("AutoSpectre_JJK0007", $"Type currently not supported",
                            $"Only classes with {Constants.AutoSpectreFormAttributeFullyQualifiedName} supported", "General", DiagnosticSeverity.Warning, true),
                        evaluationContext.Property.Locations.FirstOrDefault()));
                    return null;
                }
            }
            else
            {
                ProductionContext.ReportDiagnostic(Diagnostic.Create(
                    new("AutoSpectre_JJK0006", "Unsupported type",
                        $"Type {evaluationContext.Type} is not supported", "General", DiagnosticSeverity.Warning, true),
                    evaluationContext.Property.Locations.FirstOrDefault()));
                return null;
            }
        }

        else 
        {
            return new TextPromptBuildContext(title, type,evaluationContext.IsNullable);
        }
    }
}