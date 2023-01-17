using System;
using System.Linq;
using AutoSpectre.SourceGeneration.Extensions;
using AutoSpectre.SourceGeneration.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Spectre.Console.Rendering;

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
                if (syntaxContext.TargetSymbol is INamedTypeSymbol targetNamedType)
                {
                    var candidates = targetNamedType
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
                        .Select(x => new PropertyWithAskAttributeData(x.Property, x.Attribute!)).ToList();

                    // Check if there are any candidates
                    if (candidates.Any())
                    {
                        var propertyContexts = PropertyContextBuilderOperation.GetPropertyContexts(syntaxContext, candidates, targetNamedType, productionContext);

                        var builder = new NewCodeBuilder(targetNamedType, propertyContexts);
                        var code = builder.Code();
                        if (string.IsNullOrWhiteSpace(code))
                        {
                            productionContext.ReportDiagnostic(Diagnostic.Create(
                                new("AutoSpectre_JJK0003", "Code was empty",
                                    "No code was generated", "General", DiagnosticSeverity.Warning, true),
                                syntaxContext.TargetSymbol.Locations.FirstOrDefault()));
                        }

                        productionContext.AddSource($"{targetNamedType}AutoSpectreFactory.Generated.cs", code);
                    }
                }
                else
                {
                    productionContext.ReportDiagnostic(Diagnostic.Create(
                        new("AutoSpectre_JJK0001", "No properties are marked with attributes",
                            "No properties were marked with attributes", "General", DiagnosticSeverity.Warning,
                            true),
                        syntaxContext.TargetSymbol.Locations.FirstOrDefault()));
                }
            }
            catch (Exception ex)
            {
                productionContext.ReportDiagnostic(Diagnostic.Create(
                    new("AutoSpectre_JJK0002", "Error on processing", ex.Message, "General",
                        DiagnosticSeverity.Error, true, ex.ToString()),
                    syntaxContext.TargetSymbol.Locations.FirstOrDefault()));
            }
        });
    }
}