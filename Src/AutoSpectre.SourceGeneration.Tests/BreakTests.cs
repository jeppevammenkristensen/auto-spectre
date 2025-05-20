using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests;

public class BreakTests : AutoSpectreGeneratorTestsBase
{
    public BreakTests(ITestOutputHelper helper) : base(helper)
    {
        
    }

    [Fact]
    public void Break()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using Spectre.Console;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                        public bool DoBreak {get;set;} = true;
                        
                        
                        [Break(Condition = nameof(DoBreak))]
                        public void Break(IAnsiConsole console)
                        {
                            
                        }
                    }

                    """).OutputShouldContain("if (form.DoBreak == true)").OutputShouldContain("return form");
    }
}