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

        var result = $$"""
using Spectre.Console;
using System;
using System.Linq;

namespace {{ Type.ContainingNamespace}}    
{
    public interface I{{ Type.Name}}SpectreFactory
    {
        {{ Type.Name}}   Get({{ Type.Name}}   destination = null);
    }

    public class {{ Type.Name}}SpectreFactory : I{{ Type.Name}}SpectreFactory
    {
        public {{ Type.Name}}   Get({{ Type.Name}}   destination = null)
        {
            destination ??= new {{ name}}   ();
{{ propertySetters}}  
            return destination;
        }
    }
}
""" ;

        return SyntaxFactory.ParseCompilationUnit(result).NormalizeWhitespace().ToFullString();
    }


    private string BuildPropertySetters()
    {
        StringBuilder builder = new();
        foreach (var propertyAndAttribute in this.PropertyContexts)
        {
            builder.AppendLine(
                $"destination.{propertyAndAttribute.PropertyName} = {propertyAndAttribute.BuildContext.GenerateOutput()};");
        }

        return builder.ToString();
    }
}