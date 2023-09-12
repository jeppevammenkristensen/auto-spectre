namespace AutoSpectre.SourceGeneration.Evaluation;

public class ConfirmedNamedTypeSource
{
    public bool IsAsync => NamedTypeAnalysis.HasAnyAsyncDecoratedMethods;

    public NamedTypedAnalysis NamedTypeAnalysis { get; }
    public string? TypeConverter { get; }

    public ConfirmedNamedTypeSource(NamedTypedAnalysis namedTypeAnalysis, string? typeConverter)
    {
        NamedTypeAnalysis = namedTypeAnalysis;
        TypeConverter = typeConverter;
    }
}