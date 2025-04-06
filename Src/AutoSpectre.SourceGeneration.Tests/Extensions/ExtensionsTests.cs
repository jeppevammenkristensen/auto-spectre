using System.Reflection;
using AutoSpectre.SourceGeneration.Extensions;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests.Extensions;

public class ExtensionsTests
{
  
    
    
    private readonly ITestOutputHelper _testOutputHelper;

    public ExtensionsTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void IsEnumerableOfType_PropertiesAreEnumerable_ReturnsTrue()
    {
        var (compilation, syntaxTree) = Generate("""
            using System.Collections.Generic;

            namespace Test
            {
                public class TestClass 
                {
                    public List<string> First {get;set;}
                    public string[] Second {get;set; }
                    public string Third {get;set;}
                    public IList<string> Forth {get;set;}
                    public HashSet<int> Fifth {get;set;}
                    public IReadOnlyList<double> Sixth {get;set;}
                    public IDictionary<string,double> Seventh {get;set;}
                }
            }

            """);

        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        var tree = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var enclosingSymbol = semanticModel.GetDeclaredSymbol(tree);
        foreach (var propertySymbol in enclosingSymbol?.GetMembers().OfType<IPropertySymbol>() ?? Enumerable.Empty<IPropertySymbol>())
        {
            propertySymbol.Type.IsEnumerableOfType().isEnumerable.Should().BeTrue(propertySymbol.DeclaringSyntaxReferences.First().ToString());
        }
    }

    [Fact]
    public void IsEnumerableOfTypeButNotString_StringWillReturnFalse()
    {
        var (compilation, syntaxTree) = Generate("""
            using System.Collections.Generic;

            namespace Test
            {
                public class TestClass 
                {
                    public string First {get;set;}       
                }
            }

            """);

        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        var tree = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var enclosingSymbol = semanticModel.GetDeclaredSymbol(tree);
        if (enclosingSymbol is {})
        {
            foreach (var propertySymbol in enclosingSymbol.GetMembers().OfType<IPropertySymbol>())
            {
                propertySymbol.Type.IsEnumerableOfTypeButNotString().isEnumerable.Should().BeFalse(propertySymbol.DeclaringSyntaxReferences.First().GetSyntax().ToString());
            }
        }
    }

    
    public (Compilation compilation, SyntaxTree tree) Generate(string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var references = new List<MetadataReference>();
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            if (!assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            {
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

        references.Add(MetadataReference.CreateFromFile(typeof(TextPromptAttribute).Assembly.Location));

        var compilation = CSharpCompilation.Create("foo", new SyntaxTree[] { syntaxTree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // TODO: Uncomment this line if you want to fail tests when the injected program isn't valid _before_ running generators
        var compileDiagnostics = compilation.GetDiagnostics();

        foreach (var compileDiagnostic in compileDiagnostics)
        { 
            _testOutputHelper.WriteLine(compileDiagnostic.ToString());
        }

        compilation.GetDiagnostics().Should().NotContain(x => x.Severity == DiagnosticSeverity.Error);

        return (compilation, syntaxTree);
    }
}