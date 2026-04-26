using System;
using System.Collections.Generic;
using System.Linq;
using AutoSpectre.SourceGeneration.Extensions;
using AutoSpectre.SourceGeneration.Extensions.Specification;
using AutoSpectre.SourceGeneration.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoSpectre.SourceGeneration;

[Generator]
public class IncrementAutoSpectreGenerator : IIncrementalGenerator
{
    private static readonly Specification<ISymbol> IsPublicOrInternalSpec =
        SpecificationRecipes.IsPublic.Or(SpecificationRecipes.IsInternal);

    private HashSet<string> _memberAttributeNames = new HashSet<string>()
    {
        TextPromptAttributeNames.AttributeName, SelectPromptAttributeNames.AttributeName,
        TaskStepAttributeNames.AttributeName, BreakAttributeNames.AttributeName
    };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var syntaxNodes = context.SyntaxProvider.ForAttributeWithMetadataName(
            Constants.AutoSpectreFormAttributeFullyQualifiedName,
            (node, _) => node is ClassDeclarationSyntax, (syntaxContext, _) => syntaxContext);

        context.RegisterSourceOutput(syntaxNodes, (productionContext, syntaxContext) =>
        {
            // The matched type must be a named type symbol and be public or internal
            if (syntaxContext.TargetSymbol is INamedTypeSymbol targetNamedType &&
                IsPublicOrInternalSpec == targetNamedType)
            {
                var attribute = syntaxContext.Attributes.FirstOrDefault();
                if (attribute is null)
                    return;

                var candidates = targetNamedType
                    .GetPropertiesWithSetterAndMethods()
                    .Select(x =>
                    {
                        var (property, method) = x;
                        ISymbol symbol = property is not null ? property : method!;

                        var matchedAttribute = symbol.GetAttributes().FirstOrDefault(a =>
                            a.AttributeClass is
                            {
                                ContainingNamespace:
                                {
                                    IsGlobalNamespace: false, Name: "AutoSpectre"
                                }
                            } && _memberAttributeNames.Contains(a.AttributeClass.MetadataName));


                        return new
                        {
                            Property = property,
                            Method = method,
                            Attribute = matchedAttribute
                        };
                    })
                    .Where(x => x.Attribute != null)
                    .Select(x =>
                    {
                        return x switch
                        {
                            {Property: { } property} => new StepWithAttributeData(property, x.Attribute!),
                            {Method: { } method} => new StepWithAttributeData(method, x.Attribute!),
                            _ => throw new InvalidOperationException("Unexpected result")
                        };
                    }).ToList();

                // Check if there are any candidates
                if (candidates.Any())
                {
                    // This will evaluate the class decorated with an AutoSpectreForm attribute
                    var formEvaluation = new TargetFormEvaluator(attribute, targetNamedType, productionContext,
                        syntaxContext);

                    var singleFormEvaluationContext = formEvaluation.GetFormContext();

                    var stepContexts = StepContextBuilderOperation.GetStepContexts(syntaxContext,
                        candidates, targetNamedType, productionContext);

                    // We only check for a constructor if there are valid Steps defined.
                    if (stepContexts.Count > 0 && singleFormEvaluationContext.UsedConstructor == null)
                    {
                        productionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                                DiagnosticIds.Id0027_NoConstructor, "No usable constructor",
                                "No public constructors available", "General", DiagnosticSeverity.Error, true),
                            syntaxContext.TargetSymbol.Locations.FirstOrDefault()));
                        return;
                    }

                    var builder = new NewCodeBuilder(targetNamedType, stepContexts, singleFormEvaluationContext);
                    var code = builder.BuildCode();
                    if (string.IsNullOrWhiteSpace(code))
                    {
                        productionContext.ReportDiagnostic(Diagnostic.Create(
                            new("AutoSpectre_JJK0003", "Code was empty",
                                "No code was generated", "General", DiagnosticSeverity.Warning, true),
                            syntaxContext.TargetSymbol.Locations.FirstOrDefault()));
                    }

                    productionContext.AddSource($"{targetNamedType}AutoSpectreFactory.g.cs", code);
                    productionContext.AddSource($"SpectreFactory.{targetNamedType}.g.cs",
                        builder.BuildPartialSpectreFactory());

                    if (builder.BuildPartialCode(out var partialCode))
                    {
                        productionContext.AddSource($"{targetNamedType}PartialSummaries.g.cs", partialCode);
                    }
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
        });
    }
}