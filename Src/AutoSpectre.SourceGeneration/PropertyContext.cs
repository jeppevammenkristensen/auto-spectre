using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration
{
    /// <summary>
    /// Common shape for a single step in the generated form pipeline. A step is either a
    /// property prompt (<see cref="PropertyContext"/>) or a method invocation (<see cref="MethodContext"/>);
    /// both expose whether the step must be awaited and the <see cref="PromptBuildContext"/> that
    /// drives code generation.
    /// </summary>
    public interface IStepContext
    {
        /// <summary>True if generating this step requires the surrounding form method to be async.</summary>
        bool IsAsync { get; }

        /// <summary>The build context that knows how to emit the prompt/invocation code for this step.</summary>
        public PromptBuildContext BuildContext { get; }


        IConditionContext GetConditionContext();
    }

    /// <summary>
    /// Step context for a method-based form step (a method on the target type that the
    /// generator invokes as part of the form), pairing the method's evaluation context with
    /// its build context.
    /// </summary>
    public class MethodContext : IStepContext
    {
        public MethodContext(SingleMethodEvaluationContext evaluationContext, PromptBuildContext buildContext)
        {
            BuildContext = buildContext;
            EvaluationContext = evaluationContext;

            }

        /// <summary>Per-method evaluation state collected from attributes and symbol analysis.</summary>
        public SingleMethodEvaluationContext EvaluationContext { get;  }
        
        public IConditionContext GetConditionContext() => EvaluationContext;

        /// <inheritdoc />
        public PromptBuildContext BuildContext { get; }

        /// <summary>The Roslyn symbol for the method this step invokes.</summary>
        public IMethodSymbol MethodSymbol => EvaluationContext.Method;

        /// <summary>Convenience accessor for <see cref="MethodSymbol"/>'s name.</summary>
        public string MethodName => MethodSymbol.Name;

        /// <summary>True when the method returns a <see cref="System.Threading.Tasks.Task"/>, requiring async generation.</summary>
        public bool IsAsync => EvaluationContext.ReturnTypeIsTask;
    }

    /// <summary>
    /// Step context for a property-based form step (a property on the target type that
    /// the generator prompts the user for), carrying the property identity and its build context.
    /// </summary>
    public class PropertyContext : IStepContext
    {
        /// <summary>
        /// True if the property's value source resolves asynchronously (e.g. an async selection
        /// source). Properties themselves are sync; this reflects how the value is obtained.
        /// </summary>
        public virtual bool IsAsync { get; }//Rewrite if properties can become Async. :)

        /// <summary>Name of the property this step targets.</summary>
        public string PropertyName { get; }

        /// <summary>The Roslyn symbol for the property this step prompts for.</summary>
        public IPropertySymbol PropertySymbol { get; }

        /// <inheritdoc />
        public PromptBuildContext BuildContext { get; }

        public IConditionContext GetConditionContext()
        {
            return BuildContext.Context;
        }

        public PropertyContext(string propertyName, IPropertySymbol propertySymbol, PromptBuildContext buildContext)
        {
            PropertyName = propertyName;
            PropertySymbol = propertySymbol;
            BuildContext = buildContext;
            IsAsync = buildContext.Context.RequiresAsync;
        }
    }
}