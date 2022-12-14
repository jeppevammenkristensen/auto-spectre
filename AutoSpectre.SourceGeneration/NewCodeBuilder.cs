using AutoSpectre.SourceGeneration.Extensions;
using AutoSpectre.SourceGeneration.Models;
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
    public List<PropertyAndAttribute> PropertyAndAttributes { get; }

    public NewCodeBuilder(INamedTypeSymbol type, List<PropertyAndAttribute> propertyAndAttributes)
    {
        Type = type;
        PropertyAndAttributes = propertyAndAttributes;
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


        foreach (var (property, attributeData) in PropertyAndAttributes)
        {
            var title = attributeData.GetValue<string?>("Title") ??
                        $"Enter [green]{property.Name} [/]";
            var askType = attributeData.GetValue<AskTypeCopy>("AskType");
            var selectionType = attributeData.GetValue<string?>("SelectionSource") ?? null;


            if (GetPropertyPrompt(property, title, askType, selectionType) is { } prompt)
            {
                builder.Append($"\t\t\tdestination.{property.Name} = ");
                builder.Append(prompt);
                builder.AppendLine(";");
            }
            
        }

        return builder.ToString();
    }

    private string? GetPropertyPrompt(IPropertySymbol property, string title,
        AskTypeCopy askType, string? selectionType)
    {
        var (isNullable, type) = property.Type.GetTypeWithNullableInformation();

        var (isEnumerable, underlyingType) = property.Type.IsEnumerableOfType();

        var typeRepresentation = property.DeclaringSyntaxReferences.Select(x => x.GetSyntax()).OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault()
            ?.Type.ToString();

        if (!isEnumerable)
        {
            if (askType == AskTypeCopy.Normal)
            {
                if (type.SpecialType == SpecialType.System_Boolean)
                {
                    return $"""AnsiConsole.Confirm("{ title}")""" ;
                }
                else
                {
                    return $"AnsiConsole.Ask<{typeRepresentation}>(\"{title} \")";
                }
            }
            else if (askType == AskTypeCopy.Selection && selectionType != null)
            {
                return $"""
AnsiConsole.Prompt(
new SelectionPrompt<{ type}>()
.Title("{ title}  ")
.PageSize(10) 
.AddChoices(destination.{ selectionType}.ToArray()))
""";
            }
            else
            {
            }
        }

        return null;
    }
}