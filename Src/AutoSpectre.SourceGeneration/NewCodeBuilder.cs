using System;
using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoSpectre.SourceGeneration;

internal class NewCodeBuilder
{
    private static readonly IReadOnlyList<string> InitalNamespaces = new List<string>()
    {
        "Spectre.Console",
        "System",
        "System.Collections.Generic",
        "System.Linq",
        "System.Collections.Immutable"
    };

    public INamedTypeSymbol Type { get; }
    public List<IStepContext> StepContexts { get; }
    
    public bool HasEmptyConstructor { get; }

    public NewCodeBuilder(INamedTypeSymbol type, List<IStepContext> stepContexts)
    {
        Type = type;
        StepContexts = stepContexts;
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
{{ members}} 
            {{ ( HasEmptyConstructor ? "return destination;" : string.Empty)  }}
        }
    }
}
""" ;

        return SyntaxFactory.ParseCompilationUnit(result).NormalizeWhitespace().ToFullString();
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
        foreach (var nmSpace in InitalNamespaces.Concat(StepContexts.SelectMany(x => x.BuildContext.Namespaces())).Distinct().Where(x => x != Type.ContainingNamespace.ToString()))
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
            
            
            if (stepContext.BuildContext.Context.ConfirmedCondition is { } confirmedCondition)
            {
                var boolValue = confirmedCondition.Negate ? "false" : "true";
                
                if (confirmedCondition.SourceType == ConditionSource.Method)
                {
                    builder.AppendLine($"if (destination.{confirmedCondition.Condition}() == {boolValue})");
                    
                }
                else if (confirmedCondition.SourceType == ConditionSource.Property)
                {
                    builder.AppendLine($"if (destination.{confirmedCondition.Condition} == {boolValue})");
                }
                else
                {
                    throw new InvalidOperationException("Unexpected condition type");
                }
                
                
                builder.AppendLine("{");
                AddLine();
                builder.AppendLine("}");
            }
            else
            {
                AddLine();
            }
            
            
            

            
            
        }

        return builder.ToString();
    }
}