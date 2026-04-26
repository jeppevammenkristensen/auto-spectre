using AutoSpectre.SourceGeneration.BuildContexts;

namespace AutoSpectre.SourceGeneration.Evaluation;

public class ConfirmedSearchEnabled : ISummaryCondition
{
    public bool SearchEnabled { get; }
    public string? SearchPlaceholderText { get; }

    public ConfirmedSearchEnabled(bool searchEnabled, string? searchPlaceholderText)
    {
        SearchEnabled = searchEnabled;
        SearchPlaceholderText = searchPlaceholderText;
    }

    public void WriteToSummary(SummaryLineWriter builder)
    {
        builder.Append(" Search is enabled", true);
        if (SearchPlaceholderText is { })
        {
            builder.Append($" Placeholder: {SearchPlaceholderText}");
        }

        builder.NewLine();

    }
}