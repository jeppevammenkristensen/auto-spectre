using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace AutoSpectreAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MissingAskAttributeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AS_0001";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static string Title = "Property can be decorated with AskAttribute";

        private const string MessageFormat = "Property name '{0}' can be decorated with an AskAttribute";
        private const string Description = "Properties in a class decorated with AutoSpectreForm can be decorated with ask attribute.";
        private const string Category = "Ask";

        private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(Rule); }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(ctx =>
            {
                if (ctx.Compilation.GetTypeByMetadataName("AutoSpectre.AskAttribute") is { })
                {
                    context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
                }
            });
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
            var askAttribute = context.Compilation.GetTypeByMetadataName("AutoSpectre.AskAttribute") ??
                throw new InvalidOperationException("Expected AutoSpectre to be present");
            var form = context.Compilation.GetTypeByMetadataName("AutoSpectre.AutoSpectreForm");

            // Only do analysis on classes that are decoreated with AutoSpectreForm
            if (!namedTypeSymbol.GetAttributes()
                    .Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, form)))
            {
                return;
            }

            if (namedTypeSymbol.TypeKind == TypeKind.Class)
            {
                var accessibleProperties = namedTypeSymbol
                    .GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(x => x.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal);

                foreach (var property in accessibleProperties)
                {
                    if (property.GetAttributes().Any(x =>
                            SymbolEqualityComparer.Default.Equals(x.AttributeClass, askAttribute)))
                    {
                        continue;
                    }
                    
                    var diagnostic = Diagnostic.Create(Rule, property.Locations[0], property.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}