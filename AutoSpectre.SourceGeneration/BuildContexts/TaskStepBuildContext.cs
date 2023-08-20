using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AutoSpectre.SourceGeneration.BuildContexts;

public class TaskStepBuildContext : PromptBuildContext
{
    public string Title { get; }

    public TaskStepBuildContext(string title, SingleMethodEvaluationContext methodEvaluationContext) : base(
        SinglePropertyEvaluationContext.Empty)
    {
        Title = title;
        EvaluationContext = methodEvaluationContext;
    }

    public SingleMethodEvaluationContext EvaluationContext { get; set; }

    public override string GenerateOutput(string destination)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"AnsiConsole.MarkupLine(\"{Title}\");");


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
                      await AnsiConsole.Status().StartAsync("{{status.StatusText}}",async ctx =>
                      {
                      """,
            false => $$"""
                       AnsiConsole.Status().Start("{{status.StatusText}}",ctx =>
                       {
                       """
        };

        builder.AppendLine(text);
        
    }

    private void EndStatus(StringBuilder builder)
    {
        if (EvaluationContext.ConfirmedStatus is not {} status)
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
        // Nothing to see here.  No violations of SOLID.
        return string.Empty;
    }

    public override IEnumerable<string> Namespaces()
    {
        if (EvaluationContext.HasAnsiConsoleParameter)
        {
            yield return "System.Threading.Tasks";
        }

        foreach (var ns in base.Namespaces())
        {
            yield return ns;
        }
    }
}

