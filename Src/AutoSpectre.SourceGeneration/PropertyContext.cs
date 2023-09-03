using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration
{
    public interface IStepContext
    {
        bool IsAsync { get; }
        public PromptBuildContext BuildContext { get; }
    }

    public class MethodContext : IStepContext
    {
        public MethodContext(SingleMethodEvaluationContext evaluationContext, PromptBuildContext buildContext)
        {
            BuildContext = buildContext;
            EvaluationContext = evaluationContext;
            
            }

        public SingleMethodEvaluationContext EvaluationContext { get;  }

        public PromptBuildContext BuildContext { get; }
        public IMethodSymbol MethodSymbol => EvaluationContext.Method;
        public string MethodName => MethodSymbol.Name;
        
        public bool IsAsync => EvaluationContext.ReturnTypeIsTask;
    }
    
    public class PropertyContext : IStepContext
    {
        public virtual bool IsAsync { get; }//Rewrite if properties can become Async. :) 
        
        public string PropertyName { get; }
        public IPropertySymbol PropertySymbol { get; }
        public PromptBuildContext BuildContext { get; }

        public PropertyContext(string propertyName, IPropertySymbol propertySymbol, PromptBuildContext buildContext)
        {
            PropertyName = propertyName;
            PropertySymbol = propertySymbol;
            BuildContext = buildContext;
            IsAsync = buildContext.Context.RequiresAsync;
        }
    }
}