using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Text;
using AutoSpectre.SourceGeneration.Evaluation;
using Microsoft.CodeAnalysis;
using Spectre.Console;

namespace AutoSpectre.SourceGeneration.BuildContexts;

internal class TextPromptBuildContext : PromptBuilderContextWithPropertyContext
{
    private readonly TranslatedMemberAttributeData _memberAttributeData;
    public string Title => _memberAttributeData.Title;
    public ITypeSymbol TypeSymbol { get; }
    public bool Nullable { get; }

    public TextPromptBuildContext(TranslatedMemberAttributeData memberAttributeData, ITypeSymbol typeSymbol, bool nullable,
        SinglePropertyEvaluationContext context) : base(context)
    {
        _memberAttributeData = memberAttributeData;
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
        
        BuildCulture(builder);

        BuildSecret(builder);
        BuildDefaultValue(builder);
        BuildPromptStyle(builder);
        BuildChoices(builder);

        if (Context.ConfirmedValidator is { SingleValidation:true })
        {
            builder.AppendLine(
                $$"""
.Validate(ctx => {
    var result = {{GetStaticOrInstancePrepend(Context.ConfirmedValidator.IsStatic)}}.{{Context.ConfirmedValidator.Name}}(ctx);
    return result == null ? ValidationResult.Success() : ValidationResult.Error(result);
})
""");
        }

        builder.Append(")");
        return builder.ToString();
    }

    private void BuildChoices(StringBuilder builder)
    {
        if (Context.ConfirmedChoices is { } choices)
        {
            builder.Append(".AddChoices(destination.");
            if (choices.SourceType == ChoiceSourceType.Method)
                builder.Append($"{choices.SourceName}()");
            else if (choices.SourceType == ChoiceSourceType.Property)
                builder.Append(choices.SourceName);
            else
                throw new InvalidOperationException(
                    $"The source type {choices.SourceType} is not handled. Needs code adjustment to fix");
            builder.AppendLine(")");

            if (choices.Style is { } style)
            {
                builder.AppendLine($".ChoicesStyle(\"{style}\")");
            }

            if (choices.InvalidErrorText is { } invalidText)
            {
                builder.AppendLine($".InvalidChoiceMessage(\"{invalidText}\")");
            }

        }
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
            var name = confirmed.Instance ? $"destination.{confirmed.Name}" : $"{Context.TargetType.Name}.{confirmed.Name}";
            
            if (confirmed.Type == DefaultValueType.Property)
            {
                builder.AppendLine($".DefaultValue({name})");
            }
            else if (confirmed.Type == DefaultValueType.Method)
            {
                
                builder.AppendLine($".DefaultValue({name}());");
            }
            else
            {
                throw new InvalidOperationException($"It was unexpected. Value {confirmed.Type} was not supported");
            }

            if (confirmed.Style is { } style)
            {
                builder.AppendLine($".DefaultValueStyle(\"{style}\")");
            }
        }
    }


    private void BuildSecret(StringBuilder builder)
    {
        if (_memberAttributeData.Secret)
        {
            string mask = _memberAttributeData.Mask switch
            {
                null => "null",
                { } s => $"'{s}'",
            };


            builder.AppendLine($".Secret({mask})");
        }
    }

    private void BuildCulture(StringBuilder builder)
    {
        builder.AppendLine($".WithCulture({CodeBuildConstants.CultureVariableName})");
    }
}