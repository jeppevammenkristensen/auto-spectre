using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.BuildContexts;

internal class ReuseExistingAutoSpectreFactoryPromptBuildContext : PromptBuilderContextWithPropertyContext
{
    public override bool DeclaresVariable => true;

    public string Title { get; }
    public INamedTypeSymbol NamedTypeSymbol { get; }
    public bool IsNullable { get; }

    private string TypeNamespace { get; }

    public string FactoryInterface { get; }
    public string FactoryClassName { get; }
    public string VariableName { get; set; }


    public ReuseExistingAutoSpectreFactoryPromptBuildContext(string title, INamedTypeSymbol namedTypeSymbol,
        bool isNullable, SinglePropertyEvaluationContext context) : base(context)
    {
        Title = title;
        NamedTypeSymbol = namedTypeSymbol;
        IsNullable = isNullable;

        TypeNamespace = namedTypeSymbol.ContainingNamespace.ToString();
        FactoryClassName = namedTypeSymbol.GetSpectreFactoryClassName();
        FactoryInterface = namedTypeSymbol.GetSpectreFactoryInterfaceName();
        VariableName = namedTypeSymbol.GetSpectreVariableName();
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
            return $"""
            AnsiConsole.MarkupLine("{Title}");
            var {variableName ?? "item"} = {VariableName}.Get();
            """;
        }
        else
        {
            var usedVariable = variableName ?? "item";
            return $$"""
            {{Context.Type.ToDisplayString()}}? {{usedVariable}} = null;
            bool isValid = false;

            while (!isValid)
            { 
                AnsiConsole.MarkupLine("{{Title}}");
                {{usedVariable}} = {{VariableName}}.Get();

                if (destination.{{Context.ConfirmedValidator.Name}}({{usedVariable}}) is {} error)
                {
                    AnsiConsole.Markup("[red]{error}[/]");
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

    public override IEnumerable<string> Namespaces()
    {
            yield return TypeNamespace;
                }

    public override IEnumerable<string> CodeInitializing()
    {
            yield return $"""{FactoryInterface} {VariableName} = new {FactoryClassName}();""";
    }
}