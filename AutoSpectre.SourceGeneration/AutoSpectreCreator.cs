using System;
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
    public static class AttributeCode
    {
        //public static readonly string AutoSpectreFromAttribute = """
        //                using System;

        //    namespace AutoSpectre;

        //    /// <summary>
        //    /// Marker interface. Apply this to a class and a factory will
        //    /// </summary>
        //    public class AutoSpectreForm : Attribute
        //    {

        //    }
        //    """;

        //public static readonly string AskAttribute = """
        //                using System;

        //    namespace AutoSpectre
        //    {
        //        public class AskAttribute : Attribute
        //        {
        //            public AskAttribute()
        //            {

        //            }

        //            [Obsolete()]
        //            public AskAttribute(string? title = null)
        //            {

        //            }

        //            //public AskAttribute(string? title = null, AskType askType = AutoSpectre.AskType.Normal, string? selectionSource = null)
        //            //{
        //            //    Title = title;
        //            //    AskType = askType;
        //            //    SelectionSource = selectionSource;
        //            //}

        //            /// <summary>
        //            /// The title displayed. If nothing is defined a text including property name is displayed
        //            /// </summary>
        //            public string? Title { get; set; }

        //            /// <summary>
        //            /// The type used to ask with
        //            /// </summary>
        //            public AskType AskType { get; set; }
        //            public string? SelectionSource { get; set; }
        //        }

        //        public enum AskType
        //        {
        //            /// <summary>
        //            /// Default. 
        //            /// </summary>
        //            Normal,
        //            /// <summary>
        //            /// Presents a selection dialog
        //            /// </summary>
        //            Selection
        //        }
        //    }
            
        //    """;

        
    }


    [Generator]
    public class IncrementAutoSpectreGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //context.RegisterPostInitializationOutput(ctx =>
            //{
            //    ctx.AddSource("AskAttribute.g.cs",AttributeCode.AskAttribute);
            //    ctx.AddSource("AutoSpectreForm.g.cs",AttributeCode.AutoSpectreFromAttribute);
            //});


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

    public class CodeBuilderContext
    {
    }

    public abstract class AskBuilder
    {
        public abstract void Build(StringBuilder builder);
    }

    public class NormalAskBuilder
    {
        public NormalAskBuilder()
        {
        }
    }
}