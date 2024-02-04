using System;
using System.Collections.Generic;
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
    private HashSet<string> _memberAttributeNames = new HashSet<string>()
    {
        "AskAttribute", nameof(TextPromptAttribute), nameof(SelectPromptAttribute), nameof(TaskStepAttribute)
    };
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var syntaxNodes = context.SyntaxProvider.ForAttributeWithMetadataName(
            Constants.AutoSpectreFormAttributeFullyQualifiedName,
            (node, _) => node is ClassDeclarationSyntax, (syntaxContext, _) => syntaxContext);

        context.RegisterSourceOutput(syntaxNodes, (productionContext, syntaxContext) =>
        {
            try
            {
                if (syntaxContext.TargetSymbol is INamedTypeSymbol targetNamedType && (targetNamedType.IsPublic() || targetNamedType.IsInternal()) )
                {
                    var attribute = syntaxContext.Attributes.FirstOrDefault();
                    if (attribute is null)
                        return;
                    
                    var candidates = targetNamedType
                        .GetPropertiesWithSetterAndMethods()
                        .Select(x =>
                        {
                            var (property, method) = x;
                            ISymbol symbol = (ISymbol)property ?? method;
                            
                            var attribute = symbol!.GetAttributes().FirstOrDefault(a =>
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
                                Attribute = attribute
                            };
                        })
                        .Where(x => x.Attribute != null)
                        .Select(x =>
                        {
                            return x switch
                            {
                                { Property: { } property } => new StepWithAttributeData(property, x.Attribute!),
                                { Method: { } method } => new StepWithAttributeData(method, x.Attribute!),
                                _ => throw new InvalidOperationException("Unexpected result")
                            };
                        }).ToList();

                    // Check if there are any candidates
                    if (candidates.Any())
                    {
                        var formEvaluation = new TargetFormEvaluator(attribute, targetNamedType, productionContext,
                            syntaxContext);

                        var singleFormEvaluationContext = formEvaluation.GetFormContext();
                        
                        var stepContexts = StepContextBuilderOperation.GetStepContexts(syntaxContext,
                            candidates, targetNamedType, productionContext);
                        
                        // We only check for a constructor if there are valid Steps defined.
                        if (stepContexts.Count > 0 && singleFormEvaluationContext.UsedConstructor == null)
                        {
                            productionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                                DiagnosticIds.Id0027_NoConstructor, "No usable constructor", "No public constructors available", "General", DiagnosticSeverity.Error, true),  syntaxContext.TargetSymbol.Locations.FirstOrDefault()));
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
                    new("AutoSpectre_JJK0002", "Error on processing", ex.ToString(), "General",
                        DiagnosticSeverity.Error, true, ex.ToString()),
                    syntaxContext.TargetSymbol.Locations.FirstOrDefault()));
            }
        });
    }
}