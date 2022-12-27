using AutoSpectre.SourceGeneration.Models;

namespace AutoSpectre.SourceGeneration;

internal class TranslatedAskAttributeData
{
    public string Title { get; set; }
    public AskTypeCopy AskType { get; set; }
    public string? SelectionSource { get; set; }
}