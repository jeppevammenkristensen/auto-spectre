using FluentAssertions;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests;

/// <summary>
/// End-to-end checks that the source generator emits syntactically valid C#.
/// These guard against template-string regressions in <see cref="NewCodeBuilder"/>
/// (stray tokens, missing whitespace between identifiers, etc.) that the
/// substring-based <see cref="TestOutput.OutputShouldContain"/> assertions miss.
/// </summary>
public class GeneratedCodeCompilationTests : AutoSpectreGeneratorTestsBase
{
    public GeneratedCodeCompilationTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public void SimpleForm_GeneratedFactoryCompiles()
    {
        GetOutput("""
                  using AutoSpectre;

                  namespace Test;

                  [AutoSpectreForm]
                  public class SimpleForm
                  {
                      [TextPrompt]
                      public string Name { get; set; }
                  }
                  """).ShouldCompileWithoutErrors();
    }

    [Fact]
    public void AsyncForm_GeneratedFactoryCompiles()
    {
        GetOutput("""
                  using System.Threading.Tasks;
                  using AutoSpectre;
                  using Spectre.Console;

                  namespace Test;

                  [AutoSpectreForm]
                  public class AsyncForm
                  {
                      [TaskStep]
                      public async Task DoWork(IAnsiConsole console)
                      {
                          await Task.Delay(1);
                      }
                  }
                  """).ShouldCompileWithoutErrors();
    }

    [Fact]
    public void GeneratedPromptMethod_HasSpaceBetweenParameterTypeAndName()
    {
        // Regression: missing space between {{typeName}} and {{FormName}} produced
        // signatures like Prompt(Test.SimpleFormform) which don't compile.
        GetOutput("""
                  using AutoSpectre;

                  namespace Test;

                  [AutoSpectreForm]
                  public class SimpleForm
                  {
                      [TextPrompt]
                      public string Name { get; set; }
                  }
                  """)
            .OutputShouldContain("Prompt(Test.SimpleForm form)")
            .ShouldCompileWithoutErrors();
    }

    [Fact]
    public void GeneratedFactory_DoesNotEmitStrayClosingParenAfterNullableEnable()
    {
        // Regression: '#nullable enable)' in the template produced an invalid file header.
        var output = GetGeneratedOutput("""
                                        using AutoSpectre;

                                        namespace Test;

                                        [AutoSpectreForm]
                                        public class SimpleForm
                                        {
                                            [TextPrompt]
                                            public string Name { get; set; }
                                        }
                                        """);

        output.Should().NotContain("#nullable enable)");
    }
}
