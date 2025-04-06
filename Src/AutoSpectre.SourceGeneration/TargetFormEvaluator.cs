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

    private readonly LazyTypes _lazyTypes;

  

    public TargetFormEvaluator(AttributeData attributeData, INamedTypeSymbol targetNamedType, SourceProductionContext productionContext, GeneratorAttributeSyntaxContext syntaxContext)
    {
        AttributeData = attributeData;
        TargetNamedType = targetNamedType;
        ProductionContext = productionContext;
        SyntaxContext = syntaxContext;
        _lazyTypes = new LazyTypes(SyntaxContext.SemanticModel.Compilation);
    }

    public SingleFormEvaluationContext GetFormContext()
    {
        var translatedFormAttributeData = TranslateForm(AttributeData);
        var singleFormEvaluationContext = new SingleFormEvaluationContext();
        EvaluateCulture(translatedFormAttributeData, singleFormEvaluationContext);
        singleFormEvaluationContext.UsedConstructor = TargetNamedType.FindConstructor(_lazyTypes);
        return singleFormEvaluationContext;
    }
    
    private TranslatedFormAttributeData TranslateForm(AttributeData attributeData)
    {
        var culture = attributeData.GetAttributePropertyValue<string>(nameof(AutoSpectreForm.Culture));
        return new TranslatedFormAttributeData(culture);
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