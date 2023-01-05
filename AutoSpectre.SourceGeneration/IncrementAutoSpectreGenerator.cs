using System;
using System.Collections.Generic;
using System.Linq;
using AutoSpectre.SourceGeneration.BuildContexts;
using AutoSpectre.SourceGeneration.Extensions;
using AutoSpectre.SourceGeneration.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoSpectre.SourceGeneration;

[Generator]
public class IncrementAutoSpectreGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var syntaxNodes = context.SyntaxProvider.ForAttributeWithMetadataName(
            Constants.AutoSpectreFormAttributeFullyQualifiedName,
            (node, _) => node is ClassDeclarationSyntax, (syntaxContext, _) => syntaxContext);

        context.RegisterSourceOutput(syntaxNodes, (productionContext, syntaxContext) =>
        {
            try
            {
                if (syntaxContext.TargetSymbol is INamedTypeSymbol namedType)
                {
                    var candidates = namedType
                        .GetPropertiesWithSetter()
                        .Select(x =>
                        {
                            var attribute = x.GetAttributes().FirstOrDefault(x =>
                                x.AttributeClass is
                                {
                                    MetadataName: "AskAttribute",
                                    ContainingNamespace: {IsGlobalNamespace: false, Name: "AutoSpectre"}
                                });

                            return new
                            {
                                Property = x,
                                Attribute = attribute
                            };
                        })
                        .Where(x => x.Attribute != null)
                        .Select(x => new PropertyAndAskData(x.Property, x.Attribute!)).ToList();
                        
                    if (candidates.Any())
                    {
                        List<PropertyContext> propertyContexts = new();

                        foreach (var (property, attributeData) in candidates)
                        {
                            var (isNullable, type) = property.Type.GetTypeWithNullableInformation();

                            var (isEnumerable, underlyingType) = property.Type.IsEnumerableOfType();

                            var typeRepresentation = property.DeclaringSyntaxReferences.Select(x => x.GetSyntax()).OfType<PropertyDeclarationSyntax>()
                                .FirstOrDefault()
                                ?.Type.ToString();

                         
                                if (attributeData.AskType == AskTypeCopy.Normal)
                                {
                                    if (type.SpecialType == SpecialType.System_Boolean)
                                    {
                                        propertyContexts.Add(new PropertyContext(property.Name, property,
                                            new ConfirmPromptBuildContext(attributeData.Title, type, isNullable)));
                                    }
                                    else
                                    {
                                        propertyContexts.Add(new PropertyContext(property.Name, property,
                                            new TextPromptBuildContext(attributeData.Title, type, isNullable)));
                                    }
                                }

                                if (attributeData.AskType == AskTypeCopy.Selection)
                                {
                                    if (attributeData.SelectionSource != null)
                                    {
                                        var match = namedType
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
                                                propertyContexts.Add(new PropertyContext(property.Name, property,
                                                    new SelectionPromptBuildContext(attributeData.Title, type, isNullable,
                                                        attributeData.SelectionSource, selectionType)));
                                            }
                                            else
                                            {
                                                propertyContexts.Add(new PropertyContext(property.Name, property,new MultiSelectionBuildContext(attributeData.Title, type, isNullable, attributeData.SelectionSource, selectionType)));
                                            }
                                            
                                        }
                                        else
                                        {
                                            productionContext.ReportDiagnostic(Diagnostic.Create(
                                                new DiagnosticDescriptor("AutoSpectre_JJK0005",
                                                    "Not a valid selection source",
                                                    $"The selectionsource {attributeData.SelectionSource} was not found on type",
                                                    "General", DiagnosticSeverity.Warning, true),
                                                property.Locations.FirstOrDefault()));
                                        }
                                    }
                                }

                        }


                        var builder = new NewCodeBuilder(namedType, propertyContexts);
                        var code = builder.Code();
                        if (string.IsNullOrWhiteSpace(code))
                        {
                            productionContext.ReportDiagnostic(Diagnostic.Create(
                                new DiagnosticDescriptor("AutoSpectre_JJK0003", "Code was empty",
                                    "No code was generated", "General", DiagnosticSeverity.Warning, true),
                                syntaxContext.TargetSymbol.Locations.FirstOrDefault()));
                        }

                        productionContext.AddSource($"{namedType}AutoSpectreFactory.Generated.cs", code);
                    }
                }
                else
                {
                    productionContext.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor("AutoSpectre_JJK0001", "No properties are marked with attributes",
                            "No properties were marked with attributes", "General", DiagnosticSeverity.Warning,
                            true),
                        syntaxContext.TargetSymbol.Locations.FirstOrDefault()));
                }
            }
            catch (Exception ex)
            {
                productionContext.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("AutoSpectre_JJK0002", "Error on processing", ex.Message, "General",
                        DiagnosticSeverity.Error, true, ex.ToString()),
                    syntaxContext.TargetSymbol.Locations.FirstOrDefault()));
            }
        });
    }
}