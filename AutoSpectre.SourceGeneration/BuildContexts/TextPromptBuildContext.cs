using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Microsoft.CodeAnalysis;
using Spectre.Console;

namespace AutoSpectre.SourceGeneration.BuildContexts;

internal class TextPromptBuildContext : PromptBuilderContextWithPropertyContext
{
    private readonly TranslatedAttributeData _attributeData;
    public string Title => _attributeData.Title;
    public ITypeSymbol TypeSymbol { get; }
    public bool Nullable { get; }

    public TextPromptBuildContext(TranslatedAttributeData attributeData, ITypeSymbol typeSymbol, bool nullable,
        SinglePropertyEvaluationContext context) : base(context)
    {
        _attributeData = attributeData;
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

        BuildSecret(builder);
        BuildDefaultValue(builder);
        BuildPromptStyle(builder);

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

    private void BuildPromptStyle(StringBuilder builder)
    {
        if (Context.PromptStyle is { })
        {
            builder.AppendLine($".PromptStyle(\"{Context.PromptStyle}\")");
        }
    }

    private void BuildDefaultValue(StringBuilder builder)
    {
        if (Context.ConfirmedDefaultValue is { } confirmed)
        {
            if (confirmed.Type == DefaultValueType.Literal)
            {
                builder.AppendLine($".DefaultValue({confirmed.Name})");
            }
            else
            {
                builder.AppendLine($".DefaultValue({Context.Property.ContainingType.Name}.{confirmed.Name});");
            }

            if (confirmed.Style is { } style)
            {
                builder.AppendLine($".DefaultValueStyle(\"{style}\")");
            }
        }
    }


    private void BuildSecret(StringBuilder builder)
    {
        if (_attributeData.Secret)
        {
            string mask = _attributeData.Mask switch
            {
                null => "null",
                { } s => $"'{s}'",
            };


            builder.AppendLine($".Secret({mask})");
        }
    }
}