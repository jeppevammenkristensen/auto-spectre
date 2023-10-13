using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoSpectre.SourceGeneration.BuildContexts;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration;

internal class DumpMethodBuilder
{
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

    public string GenerateDumpMethod()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"public static void Dump(this {Type.Name} source)");
        builder.AppendLine("{");
        builder.AppendLine($"   var table = new Table();");
        builder.AppendLine("""   table.AddColumn(new TableColumn("Name"));""");
        builder.AppendLine("""   table.AddColumn(new TableColumn("Value"));""");

        var valueTuples = GenerateColumns(builder).ToList();

        foreach (var (column, value) in valueTuples)
        {
            builder.AppendLine($"table.AddRow({column}, {value});");
        }

        builder.AppendLine();
        builder.AppendLine("AnsiConsole.Write(table);");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private IEnumerable<(string column, string value)> GenerateColumns(StringBuilder builder)
    {
        foreach (var stepContext in StepContexts.OfType<PropertyContext>())
        {
            var access = $"source.{stepContext.PropertyName}";
            var title = $"""new Markup("{stepContext.PropertyName}")""";
            

            switch (stepContext.BuildContext)
            {
                case ConfirmPromptBuildContext confirmPromptBuildContext:
                    yield return (title, $"""new Markup({access}?.ToString())""");
                    break;
                case EnumPromptBuildContext enumPromptBuildContext:
                    yield return (title, $"""new Markup({access}?.ToString())""");
                    break;
                case MultiAddBuildContext multiAddBuildContext:
                    yield return (title, $"""new Markup("Currently unsupported", "Red")""");
                    // Currently not handled
                    break;
                case MultiSelectionBuildContext multiSelectionBuildContext:
                    break;
                case ReuseExistingAutoSpectreFactoryPromptBuildContext reuseExistingAutoSpectreFactoryPromptBuildContext:
                    break;
                case SelectionPromptBuildContext selectionPromptBuildContext:
                    //yield return (selectionPromptBuildContext.Title, )
                    break;
                case SelectionBaseBuildContext selectionBaseBuildContext:
                    break;
                case TextPromptBuildContext textPromptBuildContext:
                    yield return (title, $"""new Markup({access}?.ToString())""");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


        }
    }
}