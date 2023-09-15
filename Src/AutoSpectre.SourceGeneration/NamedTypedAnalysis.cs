using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration;

public class NamedTypedAnalysis
{
    public NamedTypedAnalysis(INamedTypeSymbol namedTypeSymbol, bool isDecoratedWithValidAutoSpectreForm, bool hasAnyAsyncDecoratedMethods, bool hasEmptyConstructor)
    {
        NamedTypeSymbol = namedTypeSymbol;
        IsDecoratedWithValidAutoSpectreForm = isDecoratedWithValidAutoSpectreForm;
        HasAnyAsyncDecoratedMethods = hasAnyAsyncDecoratedMethods;
        HasEmptyConstructor = hasEmptyConstructor;
    }

    public INamedTypeSymbol NamedTypeSymbol { get; }
    
    /// <summary>
    /// This return true if the given type is decorated with AutoSpectreForm and has at least
    /// one property or method decorated with a relevant attribute. It does not do a fuller analysis than
    /// that. So there can be unique scenarios where a factory has not been generated for the given type but
    /// this still is set to true. 
    /// </summary>
    public bool IsDecoratedWithValidAutoSpectreForm { get; }
    public bool HasAnyAsyncDecoratedMethods { get; }
    public bool HasEmptyConstructor { get; }
}