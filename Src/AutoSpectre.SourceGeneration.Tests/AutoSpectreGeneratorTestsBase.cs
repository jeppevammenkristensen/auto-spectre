using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Spectre.Console;
using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests;

public class CultureTests : AutoSpectreGeneratorTestsBase
{
    public CultureTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public void ClassDecoratedWithNoCultureShouldInitCultureCorrectly()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt]
                         public string SomeProperty {get;set;}
                    }
                    """).OutputShouldContain("var culture = CultureInfo.CurrentUICulture;")
                        .OutputShouldContain(".WithCulture(culture)")
                        .ShouldContainNamespace("AutoSpectre.Extensions");
    }
    
    [Fact]
    public void ClassDecoratedWithValidCultureShouldInitCultureCorrectly()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm(Culture = "da-DK")]
                    public class TestForm
                    {
                         [TextPrompt]
                         public string SomeProperty {get;set;}
                    }
                    """).OutputShouldContain("var culture = new CultureInfo(\"da-DK\")")
            .OutputShouldContain(".WithCulture(culture)")
            .ShouldContainNamespace("AutoSpectre.Extensions")
            .DumpGeneratedCode(_helper);
    }
    
    [Fact]
    public void ClassDecoratedWithInValidCultureShouldInitCultureCorrectly()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm(Culture = "chokolaaaaad")]
                    public class TestForm
                    {
                         [TextPrompt]
                         public string SomeProperty {get;set;}
                    }
                    """).ShouldHaveSourceGeneratorDiagnostic(DiagnosticIds.Id0022_CannotParseCulture);
    }
}


 

public class InheritanceTests : AutoSpectreGeneratorTestsBase
{
    public InheritanceTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public void InheritedValuesAreUsed()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm : BaseClass
                    {
                         
                    }

                    public class BaseClass
                    {
                        [TextPrompt]
                        public string InheritedValue {get;set;}
                    }
                    """).OutputShouldContain("destination.InheritedValue = ");
    }
}

public class AutoSpectreGeneratorTestsBase
{
    protected ITestOutputHelper _helper;

    protected AutoSpectreGeneratorTestsBase(ITestOutputHelper helper)
    {
        _helper = helper;
    }
    
    protected TestOutput GetOutput(string source)
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
        //generateDiagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error, "Failed: " + generateDiagnostics.FirstOrDefault()?.GetMessage());

        string output = outputCompilation.SyntaxTrees.Last().ToString();

        foreach (var diagnostic in generateDiagnostics)
        {
            _helper.WriteLine(diagnostic.ToString());
        }

        return new TestOutput(compileDiagnostics, generateDiagnostics, outputCompilation);
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