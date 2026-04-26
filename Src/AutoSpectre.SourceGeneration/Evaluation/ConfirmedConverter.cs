using AutoSpectre.SourceGeneration.BuildContexts;

namespace AutoSpectre.SourceGeneration.Evaluation;

public class ConfirmedConverter : ISummaryCondition
{
    public string Converter { get; }
    public bool IsStatic { get; }

    public ConfirmedConverter(string converter, bool isStatic)
    {
        Converter = converter;
        IsStatic = isStatic;
    }

    public void WriteToSummary(SummaryLineWriter builder)
    {
        builder.AppendLine($" Converter: {Converter}", true);
    }
}