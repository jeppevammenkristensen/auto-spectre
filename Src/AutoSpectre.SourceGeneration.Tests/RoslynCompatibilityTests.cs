using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using FluentAssertions;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests;

/// <summary>
/// Verifies that each packaged generator can load on its advertised Roslyn baseline.
/// </summary>
public class RoslynCompatibilityTests
{
    /// <summary>
    /// Inspects assembly references without loading the generator into the test host, whose
    /// selected generator variant can differ from the oldest supported baseline.
    /// </summary>
    /// <param name="variant">The packaged Roslyn variant embedded by the test project.</param>
    /// <param name="major">The supported Roslyn assembly major version.</param>
    /// <param name="minor">The supported Roslyn assembly minor version.</param>
    [Theory]
    [InlineData("roslyn4.8", 4, 8)]
    [InlineData("roslyn4.14", 4, 14)]
    [InlineData("roslyn5.3", 5, 3)]
    public void Generator_RoslynReferences_DoNotExceedSupportedBaseline(string variant, int major, int minor)
    {
        using var stream = typeof(RoslynCompatibilityTests).Assembly
            .GetManifestResourceStream($"Generator.{variant}.dll");
        stream.Should().NotBeNull($"the {variant} generator must be built and embedded");

        using var peReader = new PEReader(stream!);
        var metadata = peReader.GetMetadataReader();
        var roslynReferences = metadata.AssemblyReferences
            .Select(handle => metadata.GetAssemblyReference(handle))
            .Where(reference => metadata.GetString(reference.Name)
                is "Microsoft.CodeAnalysis" or "Microsoft.CodeAnalysis.CSharp")
            .ToArray();

        roslynReferences.Select(reference => metadata.GetString(reference.Name)).Should()
            .BeEquivalentTo(new[] { "Microsoft.CodeAnalysis", "Microsoft.CodeAnalysis.CSharp" });
        var baseline = new Version(major, minor, 0, 0);
        roslynReferences.Should().OnlyContain(reference => reference.Version <= baseline,
            $"{variant} must load on Roslyn {baseline}");
    }
}
