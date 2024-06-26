using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoSpectre.SourceGeneration.BuildContexts;
using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis;
using Spectre.Console;

namespace AutoSpectre.SourceGeneration;

internal class DumpMethodBuilder
{
    public const string SourceParameterName = "source";
    public INamedTypeSymbol Type { get; }
    public List<IStepContext> StepContexts { get; }
    public SingleFormEvaluationContext SingleFormEvaluationContext { get; }

    public DumpMethodBuilder(INamedTypeSymbol type, List<IStepContext> stepContexts,
        SingleFormEvaluationContext singleFormEvaluationContext)
    {
        Type = type;
        StepContexts = stepContexts;
        SingleFormEvaluationContext = singleFormEvaluationContext;
        // A note about hasEmptyConstructor. If there are no empty constructors we will
        // instantiate the type so it's required to pass it in. So we remove the default value and
        // change the return type to be void or Task
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GenerateDumpMethods()
    {
        var typeName = Type.FullName();
        
        var builder = new StringBuilder();
        builder.AppendLine("""
                           /// <summary>
                           /// Returns data as a IRenderable
                           /// Experimental. Might break
                           /// </summary>
                           /// <returns></returns>
                           """);

        builder.AppendLine($"public static IRenderable GenerateTable(this {typeName} {SourceParameterName})");
        builder.AppendLine("{");
        builder.AppendLine($"   var table = new Table();");
        builder.AppendLine("""   table.AddColumn(new TableColumn("Name"));""");
        builder.AppendLine("""   table.AddColumn(new TableColumn("Value"));""");

        var valueTuples = GenerateColumns().ToList();

        foreach (var (column, value) in valueTuples)
        {
            builder.AppendLine($"table.AddRow({column}, {value});");
        }

        builder.AppendLine();
        builder.AppendLine("return table;");
        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine("""
                           /// <summary>
                           /// Renders the table
                           /// Experimental. Might break
                           /// </summary>
                           /// <returns></returns>
                           """);

        builder.AppendLine($"public static void SpectreDump(this {typeName} {SourceParameterName})");
        builder.AppendLine("{");
        builder.AppendLine($"""AnsiConsole.Write({SourceParameterName}.GenerateTable());""");

        builder.AppendLine("}");
        return builder.ToString();
    }

    private IEnumerable<(string column, string value)> GenerateColumns()
    {
        foreach (var stepContext in StepContexts.OfType<PropertyContext>())
        {
            var access = $"source.{stepContext.PropertyName}{(stepContext.BuildContext.Context.IsNullable ? "?" : string.Empty)}";
            var title = $"""new Markup("{stepContext.PropertyName}")""";

            var markup = GenerateDisplayMarkup(stepContext.BuildContext, stepContext, access);
            if (markup is { })
                yield return (title, markup);
        }
    }

    private string? GenerateDisplayMarkup(PromptBuildContext promptBuildContext, PropertyContext propertyContext,
        string? access = null)
    {
        access ??= $"{SourceParameterName}.{propertyContext.PropertyName}{(propertyContext.BuildContext.Context.IsNullable ? "?" : string.Empty)}";

        return promptBuildContext switch
        {
            ConfirmPromptBuildContext => $"""new Markup({access}.ToString())""",
            EnumPromptBuildContext => $"""new Markup({access}.ToString())""",
            MultiAddBuildContext multiAddBuildContext => GenerateMultiAddContext(
                multiAddBuildContext, propertyContext, access),
            MultiSelectionBuildContext multiSelectionBuildContext => GenerateMultiSelection(multiSelectionBuildContext,
                propertyContext, access),
            ReuseExistingAutoSpectreFactoryPromptBuildContext reuse =>
                GenerateReuse(reuse, propertyContext, access),
            SelectionPromptBuildContext selectionPromptBuildContext => GenerateSelectionPrompt(
                selectionPromptBuildContext, propertyContext, access),
            TextPromptBuildContext => 
                $"""new Markup({access}.ToString())""",
            TaskStepBuildContext => null,
            _ => throw new ArgumentOutOfRangeException(nameof(promptBuildContext))
        };
    }

    private string? GenerateReuse(ReuseExistingAutoSpectreFactoryPromptBuildContext reuse,
        PropertyContext propertyContext, string access)
    {
        return $"""{access}?.GenerateTable() ?? new Text("")""";
    }

    private string? GenerateMultiSelection(MultiSelectionBuildContext multiSelectionBuildContext,
        PropertyContext propertyContext, string access)
    {
        var stringifier = GetSelectAccessor(multiSelectionBuildContext, "y");
        return GenerateRows(access, "x", stringifier, p => $"new Markup({p})");
    }

    private Func<string, string> GetSelectAccessor(SelectionBaseBuildContext context, string lambdaParameter = "x")
    {
        Func<string, string> stringifier = x => $"{x}.ToString()";

        if (context.Context.ConfirmedConverter is { } converter)
        {
            var converterAccess =
                SourceParameterName.GetStaticOrInstance(Type.FullName(),
                    converter.IsStatic);
            stringifier = x => $"{x.TrimEnd('?')} == null ? String.Empty : {converterAccess}.{converter.Converter}({x.TrimEnd('?')})";
        }

        return stringifier;
    }

    private string GenerateSelectionPrompt(SelectionPromptBuildContext selectionPromptBuildContext,
        PropertyContext propertyContext, string access)
    {
        var stringifier = GetSelectAccessor(selectionPromptBuildContext);
        return $"""new Markup({stringifier(access)})""";
    }

    private string GenerateMultiAddContext(MultiAddBuildContext multiAddBuildContext, PropertyContext property,
        string access)
    {
        return GenerateRows(access, "x", str => str,
            str => GenerateDisplayMarkup(multiAddBuildContext.BuildContext, property, str));
    }

    private string GenerateRows(string access, string lamdbaParameter, Func<string, string?> textGenerator,
        Func<string, string> markupGenerator)
    {
        var render = textGenerator(lamdbaParameter);
        if (render == null)
            return """new Text("");""";

        var builder = new StringBuilder();
        builder.AppendLine("new Rows(");
        builder.AppendLine(
            $"   {access}?.Select({lamdbaParameter} =>{render})?.Select(x => {markupGenerator("x")}).ToList() ?? new ()");
        builder.AppendLine(")");

        return builder.ToString();
    }
}