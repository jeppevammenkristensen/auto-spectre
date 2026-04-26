using AutoSpectre.SourceGeneration.BuildContexts;

namespace AutoSpectre.SourceGeneration.Evaluation;

public class ConfirmedClearOnFinish : ISummaryCondition
{
    public void WriteToSummary(SummaryLineWriter builder)
    {
        builder.AppendLine(" Clear on finish: true", true);
    }
}