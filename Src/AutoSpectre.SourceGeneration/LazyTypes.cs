using System;
using AutoSpectre.SourceGeneration.Models;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration;

/// <summary>
/// Wrapper class that supports lazily retrieving types by their name
/// </summary>
public class LazyTypes
{
    private readonly Compilation _compilation;
    private Lazy<INamedTypeSymbol?> _listGeneric;
    private Lazy<INamedTypeSymbol?> _hashSet;
    private readonly Lazy<INamedTypeSymbol?> _collection;
    private readonly Lazy<INamedTypeSymbol?> _autoSpectreForm;
    private readonly Lazy<INamedTypeSymbol?> _task;
    private readonly Lazy<INamedTypeSymbol?> _iAnsiConsole;

    public INamedTypeSymbol? ListGeneric => _listGeneric.Value;
    public INamedTypeSymbol? HashSet => _hashSet.Value;
    public INamedTypeSymbol? Collection => _collection.Value;
    public INamedTypeSymbol? AutoSpectreForm => _autoSpectreForm.Value;
    public INamedTypeSymbol? Task => _task.Value;
    public INamedTypeSymbol? IAnsiConsole => _iAnsiConsole.Value;


    public LazyTypes(Compilation compilation)
    {
        _compilation = compilation;
        _listGeneric = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName("System.Collections.Generic.List`1"));
        _hashSet = new Lazy<INamedTypeSymbol?>(() =>
            compilation.GetTypeByMetadataName("System.Collections.Generic.HashSet`1"));
        _collection = new Lazy<INamedTypeSymbol?>(() =>
            compilation.GetTypeByMetadataName("System.Collections.Generic.Collection`1"));
        _autoSpectreForm = new Lazy<INamedTypeSymbol?>(() =>
            compilation.GetTypeByMetadataName(Constants.AutoSpectreFormAttributeFullyQualifiedName));
        _task = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName("System.Threading.Tasks.Task"));
        _iAnsiConsole =
            new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName("Spectre.Console.IAnsiConsole"));
    }
}