using System.Collections.Immutable;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests;

public record TestOutput(ImmutableArray<Diagnostic> CompileDiagnostics,
    ImmutableArray<Diagnostic> GeneratorDiagnostics, Compilation OutputCompilation)
{
    public string Output => OutputCompilation.SyntaxTrees.Last().ToString();
    
    
    public TestOutput ShouldHaveSourceGeneratorDiagnostic(string id)
    {
        var testId = id;
        GeneratorDiagnostics.Should().ContainEquivalentOf(new { Id = id}, $"a source generator diagnostic matching id {id} should be present");
        return this;
    }

    public TestOutput HasNoSourceGeneratorDiagnosticWith(string id)
    {
        GeneratorDiagnostics.Should()
            .NotContain(x => x.Id == id, $"no source generator diagnostics with id {id} should be generated");
        return this;
    }

    public TestOutput OutputShouldContain(string part)
    {
        this.Output.Should().Contain(part);
        return this;
    }

    public TestOutput ShouldContainNamespace(string nameSpace)
    {
        var last = OutputCompilation.SyntaxTrees.Last();
        var compilationUnitSyntax = last.GetCompilationUnitRoot();
        var result = compilationUnitSyntax.DescendantNodes().OfType<UsingDirectiveSyntax>()
            .Any(x => x.Name?.ToString() == nameSpace);

        if (result == false)
            throw new AssertionFailedException($"Did not find a name space matching {nameSpace} in generated code");

        return this;
    }

    public TestOutput DumpGeneratedCode(ITestOutputHelper helper)
    {
        helper.WriteLine(this.Output);
        return this;
    }
}