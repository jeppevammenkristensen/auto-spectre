using System;
using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoSpectre.SourceGeneration;

public class CodeBuildConstants
{
    public const string CultureVariableName = "culture";
}

internal class NewCodeBuilder
{
    private static readonly IReadOnlyList<string> InitialNamespaces = new List<string>()
    {
        "Spectre.Console",
        "System",
        "System.Collections.Generic",
        "System.Linq",
        "System.Collections.Immutable",
        "System.Globalization",
        "AutoSpectre.Extensions"
    };

    public INamedTypeSymbol Type { get; }
    public List<IStepContext> StepContexts { get; }
    public SingleFormEvaluationContext SingleFormEvaluationContext { get; }

    public bool HasEmptyConstructor { get; }

    public NewCodeBuilder(INamedTypeSymbol type, List<IStepContext> stepContexts,
        SingleFormEvaluationContext singleFormEvaluationContext)
    {
        Type = type;
        StepContexts = stepContexts;
        SingleFormEvaluationContext = singleFormEvaluationContext;
        HasEmptyConstructor = EvaluateConstructors();
        // A note about hasEmptyConstructor. If there are no empty constructors we will
        // instantiate the type so it's required to pass it in. So we remove the default value and
        // change the return type to be void or Task
    }

    /// <summary>
    /// Helps create and fill <see cref="NewCodeBuilder"/> with values
    /// </summary>
    /// <returns></returns>
    public string Code()
    {
        var name = $"{Type.ContainingNamespace}.{Type.Name}";

        var members = BuildStepContexts();
        var isAsync = StepContexts.Any(x => x.IsAsync);

        var spectreFactoryInterfaceName = Type.GetSpectreFactoryInterfaceName();
        var spectreFactoryClassName = Type.GetSpectreFactoryClassName();
        var returnTypeName = isAsync ? $"Task<{Type.Name}>" : Type.Name;
        if (!HasEmptyConstructor)
            returnTypeName = isAsync ? "Task" : "void";
        
        var result = $$"""
{{ BuildUsingStatements() }}

namespace {{ Type.ContainingNamespace}}    
{
    /// <summary>
    /// Helps create and fill <see cref="{{ Type.Name }}"/> with values
    /// </summary>
    public interface {{spectreFactoryInterfaceName}}
    {
        {{ returnTypeName}}   Get{{ (isAsync ? "Async " : string.Empty) }}({{ Type.Name}}   destination {{ (HasEmptyConstructor ? "= null" : "")}});
    }

    /// <summary>
    /// Helps create and fill <see cref="{{ Type.Name }}"/> with values
    /// </summary>
    public class {{ spectreFactoryClassName}} : {{ spectreFactoryInterfaceName }}
    {
        public {{ (isAsync ? "async " : string.Empty) }}{{ returnTypeName}}   Get{{ (isAsync ? "Async " : string.Empty) }}({{ Type.Name}}   destination {{ (HasEmptyConstructor ? "= null" : "")}})
        {
            {{PreInitalization()}}

            {{( HasEmptyConstructor ? $"destination ??= new { name }   ();" : string.Empty )}}
            {{ InitCulture() }}
{{ members}} 
            {{ ( HasEmptyConstructor ? "return destination;" : string.Empty)  }}
        }
    }
}
""" ;

        return SyntaxFactory.ParseCompilationUnit(result).NormalizeWhitespace().ToFullString();
    }

    private string InitCulture()
    {
        var cultureInitializer = SingleFormEvaluationContext.ConfirmedCulture switch
        {
            { } cult => $"new CultureInfo(\"{cult.Culture}\")",
            _ => "CultureInfo.CurrentUICulture"
        };

        return $"var {CodeBuildConstants.CultureVariableName} = {cultureInitializer};";
    }

    private bool EvaluateConstructors()
    {
        return Type.InstanceConstructors.Any(x => x.Parameters.Length == 0);
    }

    private string PreInitalization()
    {
        var builder = new StringBuilder();

         foreach (var code in StepContexts.SelectMany(x => x.BuildContext.CodeInitializing()).Distinct())
        {
            builder.AppendLine(code);
        }

        return builder.ToString();
    }

    private string BuildUsingStatements()
    {
        var builder = new StringBuilder();
        foreach (var nmSpace in InitialNamespaces.Concat(StepContexts.SelectMany(x => x.BuildContext.Namespaces())).Distinct().Where(x => x != Type.ContainingNamespace.ToString()))
        {
            builder.AppendLine($"using {nmSpace};");
        }
        
        return builder.ToString();
    }

    private string BuildStepContexts()
    {
        StringBuilder builder = new();
        foreach (var stepContext in this.StepContexts)
        {
            void AddConditions(ConfirmedCondition condition)
            {
                var boolValue = condition.Negate ? "false" : "true";

                if (condition.SourceType == ConditionSource.Method)
                {
                    builder.AppendLine($"if (destination.{condition.Condition}() == {boolValue})");
                }
                else if (condition.SourceType == ConditionSource.Property)
                {
                    builder.AppendLine($"if (destination.{condition.Condition} == {boolValue})");
                }
                else
                {
                    throw new InvalidOperationException("Unexpected condition type");
                }


                builder.AppendLine("{");
                AddLine();
                builder.AppendLine("}");
            }

            void AddLine()
            {
                switch (stepContext)
                {
                    case MethodContext methodContext:
                        builder.AppendLine(
                            $"{methodContext.BuildContext.GenerateOutput($"destination.{methodContext.MethodName}")}");
                        break;
                    case PropertyContext propertyContext:
                        builder.AppendLine($"{propertyContext.BuildContext.GenerateOutput($"destination.{propertyContext.PropertyName}")}");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(stepContext));
                }
                
            }
            
            
            if (stepContext is PropertyContext {BuildContext.Context.ConfirmedCondition: { } confirmedCondition})
            {
                AddConditions(confirmedCondition);
            }
            else if (stepContext is MethodContext {EvaluationContext.ConfirmedCondition: { } confirmedMethodCondition})
            {
                AddConditions(confirmedMethodCondition);
            }
            else
            {
                AddLine();
            }
        }
        
        return builder.ToString();
    }
}