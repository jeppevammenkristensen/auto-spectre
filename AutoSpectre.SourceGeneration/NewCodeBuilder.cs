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

    public ITypeSymbol Type { get; }
    public List<PropertyContext> PropertyContexts { get; }

    public NewCodeBuilder(INamedTypeSymbol type, List<PropertyContext> propertyContexts)
    {
        Type = type;
        PropertyContexts = propertyContexts;
    }

    public string Code()
    {
        var name = $"{Type.ContainingNamespace}.{Type.Name}";

        var propertySetters = BuildPropertySetters();

        var spectreFactoryInterfaceName = Type.GetSpectreFactoryInterfaceName();
        var spectreFactoryClassName = Type.GetSpectreFactoryClassName();

        var result = $$"""
{{ BuildUsingStatements() }}

namespace {{ Type.ContainingNamespace}}    
{
    public interface {{spectreFactoryInterfaceName}}
    {
        {{ Type.Name}}   Get({{ Type.Name}}   destination = null);
    }

    public class {{ spectreFactoryClassName}} : {{ spectreFactoryInterfaceName }}
    {
        public {{ Type.Name}}   Get({{ Type.Name}}   destination = null)
        {
            {{PreInitalization()}}

            destination ??= new {{ name}}   ();
{{ propertySetters}}  
            return destination;
        }
    }
}
""" ;

        return SyntaxFactory.ParseCompilationUnit(result).NormalizeWhitespace().ToFullString();
    }

    private string PreInitalization()
    {
        var builder = new StringBuilder();

         foreach (var code in PropertyContexts.SelectMany(x => x.BuildContext.CodeInitializing()).Distinct())
        {
            builder.AppendLine(code);
        }

        return builder.ToString();
    }

    private string BuildUsingStatements()
    {
        var builder = new StringBuilder();
        foreach (var nmSpace in InitalNamespaces.Concat(PropertyContexts.SelectMany(x => x.BuildContext.Namespaces())).Distinct().Where(x => x != Type.ContainingNamespace.ToString()))
        {
            builder.AppendLine($"using {nmSpace};");
        }
        
        return builder.ToString();
    }

    private string BuildPropertySetters()
    {
        StringBuilder builder = new();
        foreach (var propertyAndAttribute in this.PropertyContexts)
        {
            builder.AppendLine(
                $"{propertyAndAttribute.BuildContext.GenerateOutput($"destination.{propertyAndAttribute.PropertyName}")}");
        }

        return builder.ToString();
    }
}