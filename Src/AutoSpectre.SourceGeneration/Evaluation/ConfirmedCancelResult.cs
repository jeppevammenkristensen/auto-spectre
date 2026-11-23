using System;
using System.Text;
using AutoSpectre.SourceGeneration.BuildContexts;
using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis;

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

public class ConfirmedCancelResult : SourceResult, ISummaryCondition
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

    /// <summary>
    /// Emits the cancel result, adapting enumerable sources to Spectre's list-based multi-select overload.
    /// Method sources remain deferred until cancellation.
    /// </summary>
    /// <param name="context">The prompt whose cancel result is being generated.</param>
    /// <param name="builder">The destination for the fluent configuration.</param>
    public override void GeneratePrompt(SinglePropertyEvaluationContext context, StringBuilder builder)
    {
        var source = SourcePath(context);
        var sourceType = Evaluation.MatchedSymbol switch
        {
            IMethodSymbol method => method.ReturnType,
            IPropertySymbol property => property.Type,
            IFieldSymbol field => field.Type,
            _ => null
        };
        var returnsList = sourceType is INamedTypeSymbol { Name: "List", Arity: 1 } namedType &&
                          namedType.ContainingNamespace.ToDisplayString() == "System.Collections.Generic";
        if (context.IsEnumerable && Evaluation.TargetIsEnumerable && !returnsList &&
            sourceType is not null && sourceType.IsEnumerableOfTypeButNotString().isEnumerable)
        {
            source = Evaluation.AccessType == SourceAccessType.NoParameterInvocation
                ? $"() => {source}().ToList()"
                : $"{source}.ToList()";
        }

        builder.AppendLine($".AddCancelResult({source})");
    }

    /// <summary>
    /// CancelResult has no suffix as it's either the source or pointing to a Func
    /// </summary>
    protected override string Suffix => String.Empty;

    public void WriteToSummary(SummaryLineWriter builder)
    {
        builder.Append($" Cancel result source: {Name}", true);
    }
}
