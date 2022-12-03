using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using AutoSpectre.SourceGeneration.Extensions;
using AutoSpectre.SourceGeneration.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Spectre.Console;

namespace AutoSpectre.SourceGeneration
{
    [Generator]
    public class IncrementAutoSpectreGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var syntaxNodes = context.SyntaxProvider.ForAttributeWithMetadataName(
                Constants.AutoSpectreFormAttributeFullyQualifiedName,
                (node, _) => node is ClassDeclarationSyntax, (syntaxContext, _) => syntaxContext);

            context.RegisterSourceOutput(syntaxNodes, (productionContext, syntaxContext) =>
            {
                try
                {
                    if (syntaxContext.TargetSymbol is INamedTypeSymbol namedType)
                    {
                        var propertyAndAttributes = namedType
                            .GetPropertiesWithSetter()
                            .Select(x =>
                            {
                                var attribute = x.GetAttributes().FirstOrDefault(x =>
                                    x.AttributeClass is
                                    {
                                        MetadataName: "AskAttribute",
                                        ContainingNamespace: {IsGlobalNamespace: false, Name: "AutoSpectre"}
                                    });

                                return new
                                {
                                    Property = x,
                                    Attribute = attribute
                                };
                            })
                            .Where(x => x.Attribute != null)
                            .Select(x => new PropertyAndAttribute(x.Property, x.Attribute!)).ToList();

                        if (propertyAndAttributes.Any())
                        {
                            var builder = new NewCodeBuilder(namedType, propertyAndAttributes);
                            var code = builder.Code();
                            if (string.IsNullOrWhiteSpace(code))
                            {
                                productionContext.ReportDiagnostic(Diagnostic.Create(
                                    new DiagnosticDescriptor("AutoSpectre_JJK0003", "Code was empty",
                                        "No code was generated", "General", DiagnosticSeverity.Warning, true),
                                    syntaxContext.TargetSymbol.Locations.FirstOrDefault()));
                            }

                            productionContext.AddSource($"{namedType}AutoSpectreFactory.Generated.cs", code);
                        }

                    }
                    else
                    {
                        productionContext.ReportDiagnostic(Diagnostic.Create(
                            new DiagnosticDescriptor("AutoSpectre_JJK0001", "No properties are marked with attributes",
                                "No properties were marked with attributes", "General", DiagnosticSeverity.Warning,
                                true),
                            syntaxContext.TargetSymbol.Locations.FirstOrDefault()));
                    }
                }
                catch (Exception ex)
                {
                    productionContext.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor("AutoSpectre_JJK0002", "Error on processing", ex.Message, "General",
                            DiagnosticSeverity.Error, true, ex.ToString()),
                        syntaxContext.TargetSymbol.Locations.FirstOrDefault()));
                }
            });
        }

        //internal class CodeBuilder
        //{
        //    public ClassDeclarationSyntax ClassDeclarationSyntax { get; }
        //    public ITypeSymbol Type { get; }
        //    public INamedTypeSymbol Attribute { get; }

        //    private StringBuilder _builder = new StringBuilder();

        //    public CodeBuilder(ClassDeclarationSyntax classDeclarationSyntax, ITypeSymbol type,
        //        INamedTypeSymbol attribute)
        //    {
        //        ClassDeclarationSyntax = classDeclarationSyntax;
        //        Type = type;
        //        Attribute = attribute;
        //    }

        //    public string Code()
        //    {
        //        var name = $"{Type.ContainingNamespace}.{Type.Name}";

        //        _builder.AppendLine("using Spectre.Console;");
        //        _builder.AppendLine("using System;");
        //        _builder.AppendLine();
        //        _builder.AppendLine($"namespace {Type.ContainingNamespace}");
        //        _builder.AppendLine("{");

        //        _builder.AppendLine($"   public interface I{Type.Name}SpectreFactory");
        //        _builder.AppendLine("    {");
        //        _builder.AppendLine($"        {Type.Name} Get({Type.Name} destination = null);");
        //        _builder.AppendLine("    }");


        //        _builder.AppendLine($"   public class {Type.Name}SpectreFactory : I{Type.Name}SpectreFactory");
        //        _builder.AppendLine("    {");
        //        _builder.AppendLine($"        public {Type.Name} Get({Type.Name} destination = null)");
        //        _builder.AppendLine("        {");
        //        _builder.AppendLine($"           destination ??= new {name}();");
        //        BuildPropertySetters();
        //        _builder.AppendLine($"           return destination;");

        //        _builder.AppendLine("        }");
        //        _builder.AppendLine("    }");
        //        _builder.AppendLine("}");

        //        return _builder.ToString();
        //    }

        //    private void BuildPropertySetters()
        //    {
        //        var propertySymbols = Type.GetMembers().OfType<IPropertySymbol>().ToList();

        //        foreach (var propertySymbol in propertySymbols.Where(x => x.GetAttributes().Any(AttributePredicate)))
        //        {
        //            var attribute = propertySymbol.GetAttributes().First(AttributePredicate);
        //            var title = attribute.GetValue<string?>(nameof(AskAttribute.Title), 0) ??
        //                        $"Enter [green]{propertySymbol.Name} [/]";
        //            _builder.AppendLine(
        //                $"destination.{propertySymbol.Name} = AnsiConsole.Ask<{propertySymbol.Type.Name}>(\"{title} \");");
        //        }
        //    }

        //    private bool AttributePredicate(AttributeData? attributeData)
        //    {
        //        if (attributeData?.AttributeClass is not { } attributeClass)
        //            return false;

        //        return SymbolEqualityComparer.Default.Equals(attributeClass, Attribute);
        //    }
        //}
    }

    public class PropertyAndAttribute
    {
        public void Deconstruct(out IPropertySymbol property, out AttributeData attributeData)
        {
            property = Property;
            attributeData = AttributeData;
        }

        public IPropertySymbol Property { get; }
        public AttributeData AttributeData { get; }

        public PropertyAndAttribute(IPropertySymbol property, AttributeData attributeData)
        {
            Property = property;
            AttributeData = attributeData;
        }
    }


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

namespace {{ Type.ContainingNamespace}}  
{
    public interface I{{Type.Name}}SpectreFactory
    {
        {{ Type.Name}} Get({{ Type.Name}} destination = null);
    }

    public class {{Type.Name}}SpectreFactory : I{{ Type.Name}}SpectreFactory
    {
        public {{ Type.Name}} Get({{Type.Name}} destination = null)
        {
            destination ??= new {{ name}} ();
{{ propertySetters}}
            return destination;
        }
    }
}
""" ;

            return result;
        }



        private string BuildPropertySetters()
        {
            var builder = new StringBuilder();
            

            foreach (var (property, attributeData) in PropertyAndAttributes)
            {
                var title = attributeData.GetValue<string?>(nameof(AskAttribute.Title), 0) ??
                            $"Enter [green]{property.Name} [/]";
                builder.Append($"\t\t\tdestination.{property.Name} = ");
                AppendPropertyPrompt(builder, property, title);
                builder.AppendLine(";");
                
            }

            return builder.ToString();
        }

        private void AppendPropertyPrompt(StringBuilder builder, IPropertySymbol property, string title)
        {
            var (isNullable, type) = property.Type.GetTypeWithNullableInformation();


            if (type.SpecialType == SpecialType.System_Boolean)
            {
                builder.Append($"""AnsiConsole.Confirm("{title}")""");

            }
            else
            {
                builder.Append($"AnsiConsole.Ask<{property.Type.Name}>(\"{title} \")");
            }
        }
    }    
}