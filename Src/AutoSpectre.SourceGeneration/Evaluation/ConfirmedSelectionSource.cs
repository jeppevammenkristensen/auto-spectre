using AutoSpectre.SourceGeneration.BuildContexts;

namespace AutoSpectre.SourceGeneration.Evaluation;

public class ConfirmedSelectionSource
{
    public ConfirmedSelectionSource(string name, SelectionPromptSelectionType source, bool isStatic)
    {
        Name = name;
        Source = source;
        IsStatic = isStatic;
    }

    public string Name { get; }
    public SelectionPromptSelectionType Source { get; }
    public bool IsStatic { get; }
}