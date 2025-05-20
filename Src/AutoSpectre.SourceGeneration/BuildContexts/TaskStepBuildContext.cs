using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AutoSpectre.SourceGeneration.Extensions;

namespace AutoSpectre.SourceGeneration.BuildContexts;

public class TaskStepBuildContext : PromptBuildContext
{
    private bool _generateTitle;
    
    public TaskStepBuildContext(string? title, SingleMethodEvaluationContext methodEvaluationContext) : base(
        SinglePropertyEvaluationContext.Empty, title ?? string.Empty)
    {
        _generateTitle = title is { };
        EvaluationContext = methodEvaluationContext;
    }

    public SingleMethodEvaluationContext EvaluationContext { get; set; }

    public override string GenerateOutput(string destination)
    {
        var builder = new StringBuilder();
        if (this.EvaluationContext.IsTaskStep && _generateTitle)
        {
            builder.AppendLine($"AnsiConsole.MarkupLine({GenerateTitleString()});");    
        }
        
        StartStatus(builder);

        if (EvaluationContext.ReturnTypeIsTask)
        {
            builder.AppendLine($"await {MethodInvocation(destination)};");
        }
        else
        {
            builder.AppendLine($"{MethodInvocation(destination)};");
        }
        
        EndStatus(builder);

        if (!EvaluationContext.IsTaskStep)
        {
            builder.AppendLine($"return {CodeBuildConstants.FormName};");
        }


        return builder.ToString();
    }

    private void StartStatus(StringBuilder builder)
    {
        if (EvaluationContext.ConfirmedStatus is not {} status)
        {
            return;
        }

        var text = EvaluationContext.ReturnTypeIsTask switch
        {
            true => $$"""
                      await {{BuildStatus()}}.StartAsync({{status.StatusText.GetSafeTextWithQuotes()}},async ctx =>
                      {
                      """,
            false => $$"""
                       {{BuildStatus()}}.Start({{status.StatusText.GetSafeTextWithQuotes()}},ctx =>
                       {
                       """
        };

        builder.AppendLine(text);
        
    }

    private string BuildStatus()
    {
        var builder = new StringBuilder();
        builder.Append("AnsiConsole.Status()");
        if (EvaluationContext.SpinnerStyle is not null)
        {
            builder.AppendLine();
            builder.Append($""".SpinnerStyle({EvaluationContext.SpinnerStyle.GetSafeTextWithQuotes()})""");
        }

        if (EvaluationContext.SpinnerKnownType is { } knownType)
        {
            builder.AppendLine();
            builder.Append($""".Spinner(Spinner.Known.{knownType})""");
        }

        return builder.ToString();
    }

    private void EndStatus(StringBuilder builder)
    {
        if (EvaluationContext.ConfirmedStatus is not {})
        {
            return;
        }

        builder.AppendLine("});");
    }

    private string MethodInvocation(string destination)
    {
        return $"{destination}({(EvaluationContext.HasAnsiConsoleParameter ? "AnsiConsole.Console" : string.Empty)})";
    }

    public override string PromptPart(string? variableName = null)
    {
        // Nothing to see here. No violations of SOLID.
        return string.Empty;
    }

    public override IEnumerable<string> Namespaces()
    {
        if (EvaluationContext.ReturnTypeIsTask)
        {
            yield return "System.Threading.Tasks";
        }

        foreach (var ns in base.Namespaces())
        {
            yield return ns;
        }
    }
}

