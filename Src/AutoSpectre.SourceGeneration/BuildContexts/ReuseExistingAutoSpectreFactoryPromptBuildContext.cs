using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.BuildContexts;

internal class ReuseExistingAutoSpectreFactoryPromptBuildContext : PromptBuilderContextWithPropertyContext
{
    public override bool DeclaresVariable => true;

    private ConfirmedNamedTypeSource NamedTypeSource => Context.ConfirmedNamedTypeSource ?? throw new InvalidOperationException("Source was unexpectedly null");

    public string Title { get; }
    public INamedTypeSymbol NamedTypeSymbol { get; }
    public bool IsNullable { get; }

    private string TypeNamespace { get; }

    public string FactoryInterface { get; }
    public string FactoryClassName { get; }

    /// <summary>
    /// The name of the SpectreFactory variable used to hold the instance of the factory
    /// </summary>
    public string SpectreFactoryInstanceVariableName { get; set; }


    public ReuseExistingAutoSpectreFactoryPromptBuildContext(string title, INamedTypeSymbol namedTypeSymbol,
        bool isNullable, SinglePropertyEvaluationContext context) : base(context)
    {
        if (context.ConfirmedNamedTypeSource is null)
            throw new InvalidOperationException("Expected that context.ConfirmedNamedTypeSource is not null");


        Title = title;
        NamedTypeSymbol = namedTypeSymbol;
        IsNullable = isNullable;

        TypeNamespace = namedTypeSymbol.ContainingNamespace.ToString();
        FactoryClassName = namedTypeSymbol.GetSpectreFactoryClassName();
        FactoryInterface = namedTypeSymbol.GetSpectreFactoryInterfaceName();
        SpectreFactoryInstanceVariableName = namedTypeSymbol.GetSpectreVariableName();


    }

    public override string GenerateOutput(string destination)
    {
        var builder = new StringBuilder();
        builder.AppendLine("{");
        builder.AppendLine(PromptPart());
        builder.AppendLine($"{destination} = item;");
        builder.AppendLine("}");
        return builder.ToString();
    }

    public override string PromptPart(string? variableName = null)
    {
        if (Context.ConfirmedValidator == null || !Context.ConfirmedValidator.SingleValidation)
        {
            var usedVariableName = variableName ?? "item";

            return $"""
            AnsiConsole.MarkupLine("{Title}");
            { InitializeVariable(usedVariableName) }
             { GetValueFromFactory(usedVariableName)}
            """;
        }
        else
        {
            var usedVariable = variableName ?? "item";
            return $$"""
            
            bool isValid = false;

            while (!isValid)
            { 
                AnsiConsole.MarkupLine("{{Title}}");
                {{ InitializeVariable(usedVariable) }}
                {{ GetValueFromFactory(usedVariable)}}

                if (destination.{{Context.ConfirmedValidator.Name}}({{usedVariable}}) is {} error)
                {
                    AnsiConsole.MarkupLineInterpolated($"[red]{error}[/]");
                    isValid = false;
                }
                else 
                {
                    isValid = true;
                }
            }
            """;
        }
    }

    private string InitializeVariable(string variableName)
    {
        // We are making (the probably dangerous assumption) that NamedTypeAnalysis is never null
        // when we are in this Context. Just in case we will throw an exception if it is null
      

        if (NamedTypeSource.TypeConverter is { } initializer)
        {
            return $"var {variableName} = destination.{initializer}();";
        }
        else
        {
            return $"var {variableName} = new {NamedTypeSource.NamedTypeAnalysis.NamedTypeSymbol.ToDisplayString()}();";
        }
    }

    private string GetValueFromFactory(string variableName)
    {
        if (NamedTypeSource.NamedTypeAnalysis.HasAnyAsyncDecoratedMethods)
        {
            return $"await {SpectreFactoryInstanceVariableName}.GetAsync({variableName});";
        }
        else
        {
            return $"{SpectreFactoryInstanceVariableName}.Get({variableName});";
        }
    }

    public override IEnumerable<string> Namespaces()
    {
            yield return TypeNamespace;
                }

    public override IEnumerable<string> CodeInitializing()
    {
            yield return $"""{FactoryInterface} {SpectreFactoryInstanceVariableName} = new {FactoryClassName}();""";
    }
}