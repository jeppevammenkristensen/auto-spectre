using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoSpectre.SourceGeneration
{
    [Generator]
    public class AutoSpectreCreator : ISourceGenerator
    {

        public void Initialize(GeneratorInitializationContext context)
        {
//#if DEBUG
//            if (!Debugger.IsAttached)
//            {
//                Debugger.Launch();
//            }
//#endif 

            //context.RegisterForPostInitialization((i) => i.AddSource("SpectreCreate", attributeText ) );
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                if (context.SyntaxReceiver is not SyntaxReceiver receiver || receiver.Classes.Count == 0)
                    return;

                if (!context.Compilation.ReferencedAssemblyNames.Any(x => x.Name == "Spectre.Console"))
                {
                    context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("AutoSpectre_SpectreConsoleNotFound", "AutoSpectre", "Spectre.Console not found. Please install package", "Source failed", DiagnosticSeverity.Info, true), null, DiagnosticSeverity.Info));
                    return;
                }

                var autospectre = context.Compilation.GetTypeByMetadataName("AutoSpectre.AskAttribute");
                if (autospectre == null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("AutoSpectre_NotFound", "AutoSpectre", "AutoSpectre not found. Please install package", "Source failed", DiagnosticSeverity.Info, true), null, DiagnosticSeverity.Info));
                    return;
                }

                foreach (var classes in receiver.Classes)
                {
                    var model = context.Compilation.GetSemanticModel(classes.SyntaxTree, true);
                    if (model.GetDeclaredSymbol(classes) is not ITypeSymbol type) continue;

                    var builder = new CodeBuilder(classes, type, autospectre);
                    context.AddSource($"{classes.Identifier.Text.ToString()}Factory_generated.cs", builder.Code());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }

    internal class CodeBuilder
    {
        public ClassDeclarationSyntax ClassDeclarationSyntax { get; }
        public ITypeSymbol Type { get; }
        public INamedTypeSymbol Attribute { get; }

        private StringBuilder _builder = new StringBuilder();

        public CodeBuilder(ClassDeclarationSyntax classDeclarationSyntax, ITypeSymbol type, INamedTypeSymbol attribute)
        {
            ClassDeclarationSyntax = classDeclarationSyntax;
            Type = type;
            Attribute = attribute;
        }
        
        public string Code()
        {
            var name = $"{Type.ContainingNamespace}.{Type.Name}";

            _builder.AppendLine("using Spectre.Console;");
            _builder.AppendLine("using System;");
            _builder.AppendLine();
            _builder.AppendLine($"namespace {Type.ContainingNamespace}");
            _builder.AppendLine("{");

            _builder.AppendLine($"   public interface I{Type.Name}SpectreFactory");
            _builder.AppendLine("    {");
            _builder.AppendLine($"        {Type.Name} Get({Type.Name} destination = null);");
            _builder.AppendLine("    }");


            _builder.AppendLine($"   public class {Type.Name}SpectreFactory : I{Type.Name}SpectreFactory");
            _builder.AppendLine("    {");
            _builder.AppendLine($"        public {Type.Name} Get({Type.Name} destination = null)");
            _builder.AppendLine("        {");
            _builder.AppendLine($"           destination ??= new {name}();");
            BuildPropertySetters();
            _builder.AppendLine($"           return destination;");

            _builder.AppendLine("        }");
            _builder.AppendLine("    }");
            _builder.AppendLine("}");

            return _builder.ToString();
        }

        private void BuildPropertySetters()
        {
            var propertySymbols = Type.GetMembers().OfType<IPropertySymbol>().ToList();
            
            foreach (var propertySymbol in propertySymbols.Where(x => x.GetAttributes().Any(AttributePredicate)))
            {
                var attribute = propertySymbol.GetAttributes().First(AttributePredicate);
                var title = attribute.GetValue<string?>(nameof(AskAttribute.Title), 0) ?? $"Enter [green]{propertySymbol.Name} [/]";


                _builder.AppendLine($"destination.{propertySymbol.Name} = AnsiConsole.Ask<{propertySymbol.Type.Name}>(\"{title} \");");
            }
        }

        private bool AttributePredicate(AttributeData? attributeData)
        {
            if (attributeData?.AttributeClass is not {} attributeClass)
                return false;

            return SymbolEqualityComparer.Default.Equals(attributeClass, Attribute);
        }
    }
}