using System.Text;

namespace AutoSpectre.SourceGeneration.Evaluation;

public class ConfirmedDefaultValue : SourceResult
{
    
    public ConfirmedDefaultValue(string name, SourceEvaluation evaluation) : base(name, evaluation)
    {
    }
    public override void SetPreprocessing(SinglePropertyEvaluationContext context, StringBuilder builder)
    {
        
    }

    public override void GeneratePrompt(SinglePropertyEvaluationContext context, StringBuilder builder)
    {
        builder.AppendLine($".DefaultValue({SourcePath(context)})");
    }

    protected override string Suffix => Evaluation.AccessType switch
    {
        SourceAccessType.NoParameterInvocation => "()",
        _ => string.Empty
    };

    //
    // public ConfirmedDefaultValue(DefaultValueType type, string name, string? style, bool instance)
    // {
    //     Type = type;
    //     Name = name;
    //     Instance = instance;
    // }
}