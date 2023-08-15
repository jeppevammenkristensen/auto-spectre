using AutoSpectre.SourceGeneration.BuildContexts;

namespace AutoSpectre.SourceGeneration;

public class ConfirmedSelectionSource
{
    public ConfirmedSelectionSource(string name, SelectionPromptSelectionType source)
    {
        Name = name;
        Source = source;
    }

    public string Name { get; }
    public SelectionPromptSelectionType Source { get; }
}