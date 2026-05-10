using System.Text;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests;

[TestSubject(typeof(NamespaceScope))]
public class NamespaceScopeTest
{
    [Fact]
    public void Begin_GlobalNamespace_DoesNothing()
    {
        var builder = new StringBuilder();
        var namespaceSymbol = GetNamespaceSymbol("public class Test {}");

        using (NamespaceScope.Begin(builder, namespaceSymbol))
        {
            builder.Append("content");
        }

        builder.ToString().Should().Be("content");
    }

    [Fact]
    public void Begin_SimpleNamespace_AppendsNamespaceAndBrace()
    {
        var builder = new StringBuilder();
        var namespaceSymbol = GetNamespaceSymbol("namespace Test { public class TestClass {} }");

        using (NamespaceScope.Begin(builder, namespaceSymbol))
        {
            builder.Append("  content");
        }

        builder.ToString().Should().Be($"namespace Test{Environment.NewLine}{{{Environment.NewLine}  content}}{Environment.NewLine}");
    }

    [Fact]
    public void Begin_NestedNamespace_AppendsFullNamespaceName()
    {
        var builder = new StringBuilder();
        var namespaceSymbol = GetNamespaceSymbol("namespace Test.Nested { public class TestClass {} }");

        using (NamespaceScope.Begin(builder, namespaceSymbol))
        {
            builder.Append("  content");
        }

        builder.ToString().Should().Be($"namespace Test.Nested{Environment.NewLine}{{{Environment.NewLine}  content}}{Environment.NewLine}");
    }

    private INamespaceSymbol GetNamespaceSymbol(string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create("TestAssembly", [syntaxTree], [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        var classDeclaration = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

        return classSymbol!.ContainingNamespace;
    }
}