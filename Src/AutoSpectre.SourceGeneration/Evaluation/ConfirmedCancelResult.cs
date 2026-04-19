using System;
using System.Text;
using AutoSpectre.SourceGeneration.Extensions;

namespace AutoSpectre.SourceGeneration.Evaluation;

/// <summary>
/// A base for source evaluated results. CancelResult and DefaultValueSource are examples of this
/// </summary>
public abstract class SourceResult : IConfirmedWithPreprocessing
{
    public string Name { get; }
    public SourceEvaluation Evaluation { get; }

    public bool IsStatic => Evaluation.MatchedSymbol.IsStatic;

    protected SourceResult(string name, SourceEvaluation evaluation)
    {
        Name = name;
        Evaluation = evaluation;
    }

    // NOTE! This is a placeholder for future progress where we can set variables prior to it being invocated
    public abstract void SetPreprocessing(SinglePropertyEvaluationContext context, StringBuilder builder);
  
    public abstract void GeneratePrompt(SinglePropertyEvaluationContext context, StringBuilder builder);

    /// <summary>
    /// Returns a value that is either static (SomeType.Source) or (form.Source)
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected virtual string SourcePath(SinglePropertyEvaluationContext context)
    {
        return $"{context.TargetType.GetStaticOrInstance(IsStatic)}.{Name}{Suffix}";}

    protected abstract string Suffix { get;  }
}


internal interface IConfirmedWithPreprocessing
{
    /// <summary>
    /// Set variables or anything else before the prompt is generated
    /// For instance setting a variable used;
    /// </summary>
    /// <param name="context"></param>
    /// <param name="builder"></param>
    void SetPreprocessing(SinglePropertyEvaluationContext context, StringBuilder builder);
    
    void GeneratePrompt(SinglePropertyEvaluationContext context, StringBuilder builder);

}

public class ConfirmedCancelResult : SourceResult 
{
    public ConfirmedCancelResult(string name, SourceEvaluation evaluation) : base(name, evaluation)
    {
        if (!evaluation.Valid)
        {
            throw new ArgumentException("Cannot create a CancelResult with an invalid evaluation.", nameof(evaluation));
        }
    }

    public override void SetPreprocessing(SinglePropertyEvaluationContext context, StringBuilder builder)
    {
    }

    public override void GeneratePrompt(SinglePropertyEvaluationContext context, StringBuilder builder)
    {
        builder.AppendLine($".AddCancelResult({SourcePath(context)})");
    }

    /// <summary>
    /// CancelResult has no suffix as it's either the source or pointing to a Func
    /// </summary>
    protected override string Suffix => String.Empty;
}