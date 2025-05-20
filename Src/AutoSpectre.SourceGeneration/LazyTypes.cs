using System;
using AutoSpectre.SourceGeneration.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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
    private readonly Lazy<INamedTypeSymbol?> _textPrompt;
    private readonly Lazy<INamedTypeSymbol?> _selectPrompt;
    private readonly Lazy<INamedTypeSymbol?> _taskStepPrompt;
    private readonly Lazy<INamedTypeSymbol?> _usedconstructorAttribute;
    private readonly Lazy<INamedTypeSymbol?> _breakAttribute;

    private readonly Lazy<INamedTypeSymbol?> _dateOnly;
    private readonly Lazy<INamedTypeSymbol?> _timeOnly;

    public INamedTypeSymbol? ListGeneric => _listGeneric.Value;
    public INamedTypeSymbol? HashSet => _hashSet.Value;
    public INamedTypeSymbol? Collection => _collection.Value;
    public INamedTypeSymbol? AutoSpectreForm => _autoSpectreForm.Value;
    public INamedTypeSymbol? Task => _task.Value;
    public INamedTypeSymbol? IAnsiConsole => _iAnsiConsole.Value;
    public INamedTypeSymbol? TextPrompt => _textPrompt.Value;
    public INamedTypeSymbol? SelectPrompt => _selectPrompt.Value;
    public INamedTypeSymbol? TaskStepPrompt => _taskStepPrompt.Value;
    public INamedTypeSymbol? UsedConstructorAttribute => _usedconstructorAttribute.Value;
    public INamedTypeSymbol? DateOnly => _dateOnly.Value;
    public INamedTypeSymbol? TimeOnly => _timeOnly.Value;
    public INamedTypeSymbol? BreakAttribute => _breakAttribute.Value;

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
        _textPrompt = new Lazy<INamedTypeSymbol?>(() =>
            compilation.GetTypeByMetadataName($"AutoSpectre.{nameof(TextPromptAttribute)}"));
        _selectPrompt = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName($"AutoSpectre.{nameof(SelectPromptAttribute)}"));
        _taskStepPrompt = new Lazy<INamedTypeSymbol?>(() =>
            compilation.GetTypeByMetadataName($"AutoSpectre.{nameof(TaskStepAttribute)}"));
        _usedconstructorAttribute = new Lazy<INamedTypeSymbol?>(() =>
            compilation.GetTypeByMetadataName($"AutoSpectre.{nameof(AutoSpectre.UsedConstructorAttribute)}"));
        _dateOnly = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName("System.DateOnly"));
        _timeOnly = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName("System.TimeOnly"));
        _breakAttribute = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName($"AutoSpectre.{nameof(AutoSpectre.BreakAttribute)}"));
    }

    /// <summary>
    /// For testing purposes
    /// </summary>
    /// <returns></returns>
    internal static LazyTypes Empty()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        return new LazyTypes(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}