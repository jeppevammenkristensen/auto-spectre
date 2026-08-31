using FluentAssertions;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests;

public class RoslynCompatibilityTests
{
    [Fact]
    public void Generator_RoslynReferences_DoNotExceedSupportedBaseline()
    {
        var roslynReferences = typeof(IncrementAutoSpectreGenerator).Assembly
            .GetReferencedAssemblies()
            .Where(reference => reference.Name is "Microsoft.CodeAnalysis" or "Microsoft.CodeAnalysis.CSharp")
            .ToArray();

        roslynReferences.Should().NotBeEmpty();
        roslynReferences.Should().OnlyContain(reference => reference.Version <= new Version(4, 8, 0, 0));
    }
}
