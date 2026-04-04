using System.Collections.Immutable;
using System.Linq;
using AutoSpectreAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AutoSpectreAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EmptyAutoSpectreFormAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "AS_0005";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        id: DiagnosticId,
        title: "AutoSpectreForm has no step attributes",
        messageFormat: "Class '{0}' is decorated with [AutoSpectreForm] but none of its members have an AutoSpectre step attribute (TextPrompt, SelectPrompt, TaskStep, etc.)",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A class decorated with [AutoSpectreForm] should have at least one member decorated with an AutoSpectre step attribute such as [TextPrompt], [SelectPrompt], [TaskStep], or [Break].");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;

        var autoSpectreFormAttribute = context.Compilation.GetAutoSpectreFormAttribute();
        if (autoSpectreFormAttribute == null)
            return;

        if (!namedType.HasAttribute(autoSpectreFormAttribute))
            return;

        var stepBaseType = context.Compilation.GetTypeByMetadataName("AutoSpectre.AutoSpectreStepAttribute");
        if (stepBaseType == null)
            return;

        var hasStepAttribute = namedType.GetMembers()
            .Any(member => member.GetAttributes()
                .Any(attr => InheritsFrom(attr.AttributeClass, stepBaseType)));

        if (!hasStepAttribute)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Rule,
                namedType.Locations.FirstOrDefault(),
                namedType.Name));
        }
    }

    private static bool InheritsFrom(INamedTypeSymbol? type, INamedTypeSymbol baseType)
    {
        while (type != null)
        {
            if (SymbolEqualityComparer.Default.Equals(type, baseType))
                return true;
            type = type.BaseType;
        }

        return false;
    }
}
