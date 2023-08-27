using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Spectre.Console;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests;

public class AutoSpectreGeneratorTestsBase
{
    protected ITestOutputHelper _helper;

    protected AutoSpectreGeneratorTestsBase(ITestOutputHelper helper)
    {
        _helper = helper;
    }

    protected string GetGeneratedOutput(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = new List<MetadataReference>();
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            if (!assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            {
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

        references.Add(MetadataReference.CreateFromFile(typeof(AskAttribute).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(IAnsiConsole).Assembly.Location));

        var compilation = CSharpCompilation.Create("foo", new SyntaxTree[] { syntaxTree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
           
        var compileDiagnostics = compilation.GetDiagnostics();

        foreach (var compileDiagnostic in compileDiagnostics)
        { 
            _helper.WriteLine(compileDiagnostic.ToString());
                
        }
        // Assert.False(compileDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + compileDiagnostics.FirstOrDefault()?.GetMessage());

        var generator = new IncrementAutoSpectreGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);
        generateDiagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error, "Failed: " + generateDiagnostics.FirstOrDefault()?.GetMessage());

        string output = outputCompilation.SyntaxTrees.Last().ToString();

        foreach (var diagnostic in generateDiagnostics)
        {
            _helper.WriteLine(diagnostic.ToString());
        }

        return output;
    }
}