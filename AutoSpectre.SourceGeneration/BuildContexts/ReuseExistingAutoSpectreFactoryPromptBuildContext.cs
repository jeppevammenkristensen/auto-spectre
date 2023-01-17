using System.Collections.Generic;
using System.Text;
using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.BuildContexts;

public class ReuseExistingAutoSpectreFactoryPromptBuildContext : PromptBuildContext
{
    public string Title { get; }
    public INamedTypeSymbol NamedTypeSymbol { get; }
    public bool IsNullable { get; }

    private string TypeNamespace { get; }

    public string FactoryInterface { get; }
    public string FactoryClassName { get; }
    public string VariableName { get; set; }


    public ReuseExistingAutoSpectreFactoryPromptBuildContext(string title, INamedTypeSymbol namedTypeSymbol, bool isNullable)
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

    public override string PromptPart()
    {
        return $"""
            AnsiConsole.MarkupLine("{Title}");
            var item = {VariableName}.Get();
            """;
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