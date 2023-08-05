using System;
using System.Collections.Immutable;
using System.Linq;
using System.Security;
using AutoSpectreAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AutoSpectreAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MissingValidationAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "AS_0002";

    public static string Title = "Property can add validation";

    public const string MessageFormat = "Property name '{0}' can add validation";

    public const string Description = "Properties in a class decorated with AutoSpectreForm and AskAttribute can add validation.";

    public const string Category = "Ask";

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat,
        Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);


    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(ctx =>
        {
            if (ctx.Compilation.GetAskAttribute() is { } attribute)
            {
                if (attribute.HasProperty("Validator")) 
                {
                    context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
                }
                    
            }
        });
    }

    private void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var attribute = context.Compilation.GetAskAttribute() ?? throw new InvalidOperationException("Expected AutoSpectre to be present");
        var form = context.Compilation.GetAutoSpectreFormAttribute()!;

        if (context.Symbol is not INamedTypeSymbol namedType) return;

        if (namedType.HasAttribute(form))
        {
            foreach (var propertySymbol in namedType.GetProperties()
                         .Where(x => x.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal))
            {
                CheckProperty(propertySymbol, namedType, attribute, context);
            }
        }
    }

    private void CheckProperty(IPropertySymbol propertySymbol, INamedTypeSymbol namedTypeSymbol,
        INamedTypeSymbol attribute, SymbolAnalysisContext context)
    {
        if (propertySymbol.GetAttribute(attribute) is {} attributeData)
        {
            // if has constructor argument with name Validator
            if (attributeData.NamedArguments.Any(x => x.Key == "Validator"))
            {
                return;
            }

            if (namedTypeSymbol
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .FirstOrDefault(x => x.Name.Equals($"{propertySymbol.Name}Validator")) is { } methodSymbol)

            {
                var (isEnumerable, _) = propertySymbol.Type.IsEnumerableOfTypeButNotString();

                if (isEnumerable)
                {
                    if (methodSymbol.Parameters.Length != 2)
                    {
                        var diagnostic = Diagnostic.Create(Rule, propertySymbol.Locations[0], propertySymbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
                else
                {
                    if (methodSymbol.Parameters.Length != 1)
                    {
                        var diagnostic = Diagnostic.Create(Rule, propertySymbol.Locations[0], propertySymbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
            else
            {
                var diagnostic = Diagnostic.Create(Rule, propertySymbol.Locations[0], propertySymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);


}