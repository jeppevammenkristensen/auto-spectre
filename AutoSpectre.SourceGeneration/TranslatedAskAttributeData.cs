using AutoSpectre.SourceGeneration.Models;

namespace AutoSpectre.SourceGeneration;

public class TranslatedAskAttributeData
{
    public TranslatedAskAttributeData(AskTypeCopy askType, string? selectionSource, string title, string? converter)
    {
        AskType = askType;
        SelectionSource = selectionSource;
        Title = title;
        Converter = converter;
    }

    public string Title { get;  }
    public string? Converter { get; }
    public AskTypeCopy AskType { get;  }
    public string? SelectionSource { get; }
}