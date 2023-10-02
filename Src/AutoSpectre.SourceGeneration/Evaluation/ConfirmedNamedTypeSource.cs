namespace AutoSpectre.SourceGeneration.Evaluation;

public class ConfirmedNamedTypeSource
{
    public bool IsAsync => NamedTypeAnalysis.HasAnyAsyncDecoratedMethods;

    public NamedTypedAnalysis NamedTypeAnalysis { get; }
    public string? TypeConverter { get; }
    public bool IsStatic { get; }

    public ConfirmedNamedTypeSource(NamedTypedAnalysis namedTypeAnalysis, string? typeConverter,
        bool isStatic)
    {
        NamedTypeAnalysis = namedTypeAnalysis;
        TypeConverter = typeConverter;
        IsStatic = isStatic;
    }
}