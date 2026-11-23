using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests;

/// <summary>
/// Verifies generated APIs do not expose forms beyond their declared accessibility.
/// </summary>
public class FactoryAccessibilityTests : AutoSpectreGeneratorTestsBase
{
    public FactoryAccessibilityTests(ITestOutputHelper helper) : base(helper)
    {
    }

    /// <summary>
    /// Covers synchronous and asynchronous factories with both construction paths.
    /// </summary>
    /// <param name="accessibility">The form's declared accessibility.</param>
    /// <param name="isAsync">Whether a task step requires asynchronous generation.</param>
    /// <param name="requiresArgument">Whether construction requires a caller-supplied form.</param>
    [Theory]
    [InlineData("internal", false, false)]
    [InlineData("internal", false, true)]
    [InlineData("internal", true, false)]
    [InlineData("internal", true, true)]
    [InlineData("public", false, false)]
    [InlineData("public", false, true)]
    [InlineData("public", true, false)]
    [InlineData("public", true, true)]
    public void GeneratedApisMatchFormAccessibility(string accessibility, bool isAsync, bool requiresArgument)
    {
        var constructor = requiresArgument ? "public TestForm(int seed) { }" : string.Empty;
        var step = isAsync
            ? "[TaskStep] public Task Work() => Task.CompletedTask;"
            : "[TextPrompt] public string Name { get; set; } = \"\";";
        var output = GetOutput($$"""
                                 using AutoSpectre;
                                 using System.Threading.Tasks;
                                 namespace Test;

                                 [AutoSpectreForm]
                                 {{accessibility}} class TestForm
                                 {
                                     {{constructor}}
                                     {{step}}
                                 }
                                 """);

        output.ShouldCompileWithoutErrors();
        var expected = accessibility == "public" ? Accessibility.Public : Accessibility.Internal;
        var compilation = output.OutputCompilation;
        compilation.GetTypeByMetadataName("Test.TestFormSpectreFactory")!
            .DeclaredAccessibility.Should().Be(expected);
        compilation.GetTypeByMetadataName("Test.ITestFormSpectreFactory")!
            .DeclaredAccessibility.Should().Be(expected);
        var methods = compilation.GetTypeByMetadataName("AutoSpectre.SpectreFactory")!
            .GetMembers().OfType<IMethodSymbol>().Where(method => method.MethodKind == MethodKind.Ordinary).ToList();
        methods.Should().NotBeEmpty();
        methods.Should().OnlyContain(method => method.DeclaredAccessibility == expected);
    }
}