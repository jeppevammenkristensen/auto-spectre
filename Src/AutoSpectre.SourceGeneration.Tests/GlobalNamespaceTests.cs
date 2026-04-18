using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests;

public class GlobalNamespaceTests : AutoSpectreGeneratorTestsBase
{
    public GlobalNamespaceTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public void FormInGlobalNamespaceGeneratesValidCode()
    {
        var output = GetOutput("""
                               using AutoSpectre;

                               [AutoSpectreForm]
                               public class TestForm
                               {
                                   [TextPrompt]
                                   public string Name { get; set; }
                               }
                               """);

        var generatedFactory = output.Output;
        generatedFactory.Should().NotContain("<global namespace>");
        generatedFactory.Should().NotContain("namespace ");
        generatedFactory.Should().Contain("class TestFormSpectreFactory");

        var partialFactory = output.OutputCompilation.SyntaxTrees.Last().ToString();
        partialFactory.Should().NotContain("<global namespace>");
        partialFactory.Should().NotContain("using ;");
    }
}
