using System;
using System.Collections.Generic;
using System.Linq;
using AutoSpectre.SourceGeneration.BuildContexts;
using AutoSpectre.SourceGeneration.Extensions;
using AutoSpectre.SourceGeneration.Models;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration;

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
            var (isNullable, type) = property.Type.GetTypeWithNullableInformation();
            var (isEnumerable, underlyingType) = property.Type.IsEnumerableOfTypeButNotString();

            if (attributeData.AskType == AskTypeCopy.Normal)
            {
                if (!isEnumerable)
                {
                    if (GetNormalPromptBuildContext(attributeData.Title, type, isNullable, property) is
                        { } promptBuildContext)
                    {
                        propertyContexts.Add(new(property.Name, property,
                            promptBuildContext));
                    }
                }
                else
                {
                    if (GetNormalPromptBuildContext(attributeData.Title, underlyingType, isNullable, property) is
                        { } promptBuildContext)
                    {
                        propertyContexts.Add(new(property.Name, property, new MultiAddBuildContext(type, underlyingType, types, promptBuildContext)));
                    }
                }
            }

            if (attributeData.AskType == AskTypeCopy.Selection)
            {
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
                        if (!isEnumerable)
                        {
                            propertyContexts.Add(new(property.Name, property,
                                new SelectionPromptBuildContext(attributeData.Title, type, isNullable,
                                    attributeData.SelectionSource, selectionType)));
                        }
                        else
                        {
                            propertyContexts.Add(new(property.Name, property,
                                new MultiSelectionBuildContext(title: attributeData.Title,
                                    typeSymbol: type, underlyingSymbol: underlyingType,
                                    nullable: isNullable,
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

    public PromptBuildContext? GetNormalPromptBuildContext(string title, ITypeSymbol type, bool isNullable, IPropertySymbol property)
    {
        if (type.SpecialType == SpecialType.System_Boolean)
        {
            return new ConfirmPromptBuildContext(title, type, isNullable);
        }
        else if (type.SpecialType == SpecialType.System_Enum)
        {
            return new EnumPromptBuildContext(title, type, isNullable);
        }
        else if (type.SpecialType == SpecialType.None)
        {
            if (type is INamedTypeSymbol namedType)
            {
                

                if (namedType.GetAttributes().FirstOrDefault(x =>
                        SymbolEqualityComparer.Default.Equals(x.AttributeClass,Types.AutoSpectreForm)) is { })
                {
                    return new ReuseExistingAutoSpectreFactoryPromptBuildContext(title, namedType, isNullable);
                }
                else
                {
                    ProductionContext.ReportDiagnostic(Diagnostic.Create(
                        new("AutoSpectre_JJK0007", $"Type currently not supported",
                            $"Only classes with {Constants.AutoSpectreFormAttributeFullyQualifiedName} supported", "General", DiagnosticSeverity.Warning, true),
                        property.Locations.FirstOrDefault()));
                    return null;
                }
            }
            else
            {
                ProductionContext.ReportDiagnostic(Diagnostic.Create(
                    new("AutoSpectre_JJK0006", "Unsupported type",
                        $"Type {type} is not supported", "General", DiagnosticSeverity.Warning, true),
                    type.Locations.FirstOrDefault()));
                return null;
            }
        }

        else 
        {
            return new TextPromptBuildContext(title, type, isNullable);
        }
    }
}