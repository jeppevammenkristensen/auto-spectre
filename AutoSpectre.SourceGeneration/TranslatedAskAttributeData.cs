using AutoSpectre.SourceGeneration.Models;

namespace AutoSpectre.SourceGeneration;

public class TranslatedAskAttributeData
{
    public TranslatedAskAttributeData(AskTypeCopy askType, string? selectionSource, string title)
    {
        AskType = askType;
        SelectionSource = selectionSource;
        Title = title;
    }

    public string Title { get;  }
    public AskTypeCopy AskType { get;  }
    public string? SelectionSource { get; }
}