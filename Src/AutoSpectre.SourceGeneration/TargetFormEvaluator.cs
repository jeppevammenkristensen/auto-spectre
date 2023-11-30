using System.Globalization;
using System.Linq;
using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration;

/// <summary>
/// Evaluates only the containing class that has the AutoSpectreForm
/// on it
/// </summary>
public class TargetFormEvaluator
{
    public AttributeData AttributeData { get; }
    public INamedTypeSymbol TargetNamedType { get; }
    public SourceProductionContext ProductionContext { get; }
    public GeneratorAttributeSyntaxContext SyntaxContext { get; }
    public INamedTypeSymbol NamedTypeSymbol { get; }

  

    public TargetFormEvaluator(AttributeData attributeData, INamedTypeSymbol targetNamedType, SourceProductionContext productionContext, GeneratorAttributeSyntaxContext syntaxContext)
    {
        AttributeData = attributeData;
        TargetNamedType = targetNamedType;
        ProductionContext = productionContext;
        SyntaxContext = syntaxContext;
    }

    public SingleFormEvaluationContext GetFormContext()
    {
        var translatedFormAttributeData = TranslateForm(AttributeData);
        var singleFormEvaluationContext = new SingleFormEvaluationContext();
        EvaluateCulture(translatedFormAttributeData, singleFormEvaluationContext);
        singleFormEvaluationContext.DisableDumpMethod = translatedFormAttributeData.DisableDump;
        return singleFormEvaluationContext;
    }
    
    private TranslatedFormAttributeData TranslateForm(AttributeData attributeData)
    {
        var culture = attributeData.GetAttributePropertyValue<string>(nameof(AutoSpectreForm.Culture));
        var disableDump = attributeData.GetAttributePropertyValue<bool>(nameof(AutoSpectreForm.DisableDump));
        return new TranslatedFormAttributeData(culture, disableDump);
    }

    private void EvaluateCulture(TranslatedFormAttributeData translatedFormAttributeData, SingleFormEvaluationContext evaluationContext)
    {
        if (translatedFormAttributeData.Culture is { } culture)
        {
            try
            {
                _ = new CultureInfo(culture);
            }
            catch (CultureNotFoundException)
            {
                ProductionContext.ReportDiagnostic(Diagnostic.Create(
                    new(DiagnosticIds.Id0022_CannotParseCulture, $"Could not parse culture {culture}", $"Culture with value {culture} could not be parsed", "General",
                        DiagnosticSeverity.Error, true),
                    TargetNamedType.Locations.FirstOrDefault()));
            }

            evaluationContext.ConfirmedCulture = new ConfirmedCulture(culture);
        }
    }
}