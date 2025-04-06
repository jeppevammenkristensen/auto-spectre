﻿using System;
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
using static AutoSpectre.SourceGeneration.CodeBuildConstants;

namespace AutoSpectre.SourceGeneration;

public class CodeBuildConstants
{
    public const string CultureVariableName = "culture";
    public const string FormName = "form";
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
        "AutoSpectre",
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
        // Gets the type name
        var typeName = Type.FullName();
        var name = $"{Type.ContainingNamespace}.{Type.FullName()}";
        
        var members = BuildStepContexts();
        var isAsync = IsAsync;
        var interfaceInheritcance = isAsync ? $"IAsyncSpectreFactory<{typeName}>" : $"ISpectreFactory<{typeName}>";
        var implementationInheritance = isAsync ? $"AsyncSpectreFactoryBase<{typeName}>, {SpectreFactoryInterfaceName}" : $"{SpectreFactoryInterfaceName}";
        
        var returnTypeName = isAsync ? $"Task<{Type.FullName()}>" : Type.FullName();
        // if (!CanBeInitializedWithoutParameters)
        //     returnTypeName = isAsync ? "Task" : "void";
        
        var result = $$"""
// <auto-generated/>
#pragma warning disable
#nullable enable
{{ BuildUsingStatements() }}

namespace {{ Type.ContainingNamespace}}    
{
    /// <summary>
    /// Helps create and fill <see cref="{{ typeName }}"/> with values
    /// </summary>
    public interface {{SpectreFactoryInterfaceName}} : {{ interfaceInheritcance }}
    {
        {{ returnTypeName }}   Prompt{{ (isAsync ? "Async " : string.Empty) }}({{ typeName}}   destination);
    }

    /// <summary>
    /// Helps create and fill <see cref="{{ typeName }}"/> with values
    /// </summary>
    public class {{ SpectreFactoryClassName}} : {{ implementationInheritance }}
    {
        public {{ (isAsync ? "override async " : string.Empty) }}{{ returnTypeName}}  Prompt{{ (isAsync ? "Async " : string.Empty) }}({{ typeName}} {{FormName}})
        {
            if ({{FormName}} == null) throw new ArgumentNullException(nameof({{FormName}}));
        
            {{PreInitalization()}}
         
            {{ InitCulture() }}
{{ members}} 
            return {{FormName}};
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
        var typeName = Type.FullName();
        var type = IsAsync ? $"Task<{typeName}>" : typeName;
        
        var extensionmethodName = IsAsync ? "SpectrePromptAsync" : "SpectrePrompt";

        var call = IsAsync ? "await factory.PromptAsync(source);" : "factory.Prompt(source);";

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
                     public static {{GetReturnedType(type,IsAsync)}} {{extensionmethodName}} (this {{typeName}} source)
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
                    builder.AppendLine($"if ({FormName}.{condition.Condition}() == {boolValue})");
                }
                else if (condition.SourceType == ConditionSource.Property)
                {
                    builder.AppendLine($"if ({FormName}.{condition.Condition} == {boolValue})");
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
                            $"{methodContext.BuildContext.GenerateOutput($"{FormName}.{methodContext.MethodName}")}");
                        break;
                    case PropertyContext propertyContext:
                        builder.AppendLine($"{propertyContext.BuildContext.GenerateOutput($"{FormName}.{propertyContext.PropertyName}")}");
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
