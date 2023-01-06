using System;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration;

public class Types
{
    private readonly Compilation _compilation;
    private Lazy<INamedTypeSymbol?> _listGeneric;
    private Lazy<INamedTypeSymbol?> _hashSet;
    private readonly Lazy<INamedTypeSymbol?> _collection;
    public INamedTypeSymbol? ListGeneric => _listGeneric.Value;
    public INamedTypeSymbol? HashSet => _hashSet.Value;
    public INamedTypeSymbol? Collection => _collection.Value;


    public Types(Compilation compilation)
    {
        _compilation = compilation;
        _listGeneric = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName("System.Collections.Generic.List`1"));
        _hashSet = new Lazy<INamedTypeSymbol?>(() =>
            compilation.GetTypeByMetadataName("System.Collections.Generic.HashSet`1"));
        _collection = new Lazy<INamedTypeSymbol?>(() =>
            compilation.GetTypeByMetadataName("System.Collections.Generic.Collection`1"));
    }
}