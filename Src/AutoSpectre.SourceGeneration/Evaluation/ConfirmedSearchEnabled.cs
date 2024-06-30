namespace AutoSpectre.SourceGeneration.Evaluation;

public class ConfirmedSearchEnabled
{
    public bool SearchEnabled { get; }
    public string? SearchPlaceholderText { get; }

    public ConfirmedSearchEnabled(bool searchEnabled, string? searchPlaceholderText)
    {
        SearchEnabled = searchEnabled;
        SearchPlaceholderText = searchPlaceholderText;
    }
}