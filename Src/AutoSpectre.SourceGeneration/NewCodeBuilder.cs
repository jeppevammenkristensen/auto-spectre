using System;
using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoSpectre.SourceGeneration.Evaluation;
using Spectre.Console;
using Spectre.Console.Rendering;

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
        "AutoSpectre.Extensions",
        "Spectre.Console.Rendering"
    };

    public INamedTypeSymbol Type { get; }
    public List<IStepContext> StepContexts { get; }
    public SingleFormEvaluationContext SingleFormEvaluationContext { get; }

    public bool CanBeInitializedWithoutParameters { get; }

    public NewCodeBuilder(INamedTypeSymbol type, List<IStepContext> stepContexts,
        SingleFormEvaluationContext singleFormEvaluationContext)
    {
        Type = type;
        StepContexts = stepContexts;
        SingleFormEvaluationContext = singleFormEvaluationContext;
        if (SingleFormEvaluationContext.UsedConstructor == null)
            throw new InvalidOperationException("UsedConstructor was null. This should not have been called");
        
        CanBeInitializedWithoutParameters = EvaluateIfCanBeInitalizedWithoutParameters();
        // A note about hasEmptyConstructor. If there are no empty constructors we will
        // instantiate the type so it's required to pass it in. So we remove the default value and
        // change the return type to be void or Task
    }

    /// <summary>
    /// Helps create and fill <see cref="NewCodeBuilder"/> with values
    /// </summary>
    /// <returns>A string representation of the generated code</returns>
    public string BuildCode()
    {
        var name = $"{Type.ContainingNamespace}.{Type.Name}";
        var members = BuildStepContexts();
        var isAsync = IsAsync;
        
        var returnTypeName = isAsync ? $"Task<{Type.Name}>" : Type.Name;
        if (!CanBeInitializedWithoutParameters)
            returnTypeName = isAsync ? "Task" : "void";
        
        var result = $$"""
{{ BuildUsingStatements() }}

namespace {{ Type.ContainingNamespace}}    
{
    /// <summary>
    /// Helps create and fill <see cref="{{ Type.Name }}"/> with values
    /// </summary>
    public interface {{SpectreFactoryInterfaceName}}
    {
        {{ returnTypeName}}   Get{{ (isAsync ? "Async " : string.Empty) }}({{ Type.Name}}   destination {{ (CanBeInitializedWithoutParameters ? "= null" : "")}});
    }

    /// <summary>
    /// Helps create and fill <see cref="{{ Type.Name }}"/> with values
    /// </summary>
    public class {{ SpectreFactoryClassName}} : {{ SpectreFactoryInterfaceName }}
    {
        public {{ (isAsync ? "async " : string.Empty) }}{{ returnTypeName}}   Get{{ (isAsync ? "Async " : string.Empty) }}({{ Type.Name}}   destination {{ (CanBeInitializedWithoutParameters ? "= null" : "")}})
        {
            {{PreInitalization()}}

            {{( CanBeInitializedWithoutParameters ? $"destination ??= new { name }   ();" : string.Empty )}}
            {{ InitCulture() }}
{{ members}} 
            {{ ( CanBeInitializedWithoutParameters ? "return destination;" : string.Empty)  }}
        }
    }
    
    {{ GenerateExtensionMethodsClass() }}
    
}
""" ;
        
        
    string GenerateExtensionMethodsClass()
    {
        var generateDumpMethod = SingleFormEvaluationContext.DisableDumpMethod
            ? string.Empty
            : new DumpMethodBuilder(Type,
                StepContexts,
                SingleFormEvaluationContext).GenerateDumpMethods();
        
         return GeneratePromptExtensionMethod(generateDumpMethod);
    }



    return SyntaxFactory.ParseCompilationUnit(result).NormalizeWhitespace().ToFullString();
    }

    private bool IsAsync
    {
        get
        {
            var isAsync = StepContexts.Any(x => x.IsAsync);
            return isAsync;
        }
    }

    private string SpectreFactoryClassName
    {
        get
        {
            var spectreFactoryClassName = Type.GetSpectreFactoryClassName();
            return spectreFactoryClassName;
        }
    }

    private string SpectreFactoryInterfaceName
    {
        get
        {
            var spectreFactoryInterfaceName = Type.GetSpectreFactoryInterfaceName();
            return spectreFactoryInterfaceName;
        }
    }
    
    private string GeneratePromptExtensionMethod(string generateDumpMethod)
    {
        var type = IsAsync ? $"Task<{Type.Name}>" : Type.Name;
        
        var extensionmethodName = IsAsync ? "SpectrePromptAsync" : "SpectrePrompt";

        var call = IsAsync ? "await factory.GetAsync(source);" : "factory.Get(source);";

        if (CanBeInitializedWithoutParameters)
        {
            call = $"return {call}";
        }
        else
        {
            call = $"""
                    {call}
                    return source;
                    """;
        }
        
        return $$"""
                  public static class {{ SpectreFactoryClassName }}Extensions
                 {
                     public static {{GetReturnedType(type,IsAsync)}} {{extensionmethodName}} (this {{Type.Name}} source)
                     {
                          {{SpectreFactoryClassName}} factory = new();
                          {{ call }}
                     }
                     
                     {{ generateDumpMethod }}
                 }
                 """;
    }

    private string GetReturnedType(string type, bool isAsync)
    {
        return $"{(isAsync ? "async" : "")} {type}";
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

    private bool EvaluateIfCanBeInitalizedWithoutParameters()
    {
        return SingleFormEvaluationContext.UsedConstructor!.Parameters.Length == 0  && !Type.GetAllProperties().Any(x => x.IsRequired);
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
