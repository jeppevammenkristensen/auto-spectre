using System.Collections.Immutable;
using FluentAssertions;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.Tests;

public record TestOutput(string Output, ImmutableArray<Diagnostic> CompileDiagnostics,
    ImmutableArray<Diagnostic> GeneratorDiagnostics)
{
    public TestOutput ShouldHaveSourceGeneratorDiagnostic(string id)
    {
        GeneratorDiagnostics.Should().Contain(x => x.Id == id, $"a source generator diagnostic with id {id}");
        return this;
    }

    public TestOutput HasNoSourceGeneratorDiagnosticWith(string id)
    {
        GeneratorDiagnostics.Should()
            .NotContain(x => x.Id == id, $"no source generator diagnostics with id {id} should be generated");
        return this;
    }

    public TestOutput OutputShouldContains(string part)
    {
        this.Output.Should().Contain(part);
        return this;
    }
}