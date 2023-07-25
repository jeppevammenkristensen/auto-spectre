using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Spectre.Console;

namespace AutoSpectre.SourceGeneration.BuildContexts;

internal class TextPromptBuildContext : PromptBuilderContextWithPropertyContext
{
    public string Title { get; }
    public ITypeSymbol TypeSymbol { get; }
    public bool Nullable { get; }

    public TextPromptBuildContext(string title, ITypeSymbol typeSymbol, bool nullable,
        SinglePropertyEvaluationContext context) : base(context)
    {
        Title = title;
        TypeSymbol = typeSymbol;
        Nullable = context.IsNullable;
    }

    public override string GenerateOutput(string destination)
    {
        return $"{destination} = {PromptPart()};";
    }

    public override string PromptPart(string? variableName = null)
    {
        var syntax = TypeSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().ToString() ?? TypeSymbol.ToDisplayString();
        StringBuilder builder = new();
        builder.AppendLine("AnsiConsole.Prompt(");
        builder.AppendLine($"""new TextPrompt<{syntax}>("{Title}")""");
        if (Nullable)
        {
            builder.AppendLine(".AllowEmpty()");
        }

        if (Context.ConfirmedValidator is { SingleValidation:true })
        {
            builder.AppendLine(
                $$"""
.Validate(ctx => {
    var result = destination.{{Context.ConfirmedValidator.Name}}(ctx);
    return result == null ? ValidationResult.Success() : ValidationResult.Error(result);
})
""");
        }

        builder.Append(")");
        return builder.ToString();
    }
}