using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests;

public class TaskStepGeneratorTests : AutoSpectreGeneratorTestsBase
{
    [Fact]
    public void TaskStepVoidWithAnsiConsoleMethodGeneratesExpected()
    {
        GetGeneratedOutput($$"""
                             using AutoSpectre;
                             using System.Collections.Generic;

                             namespace Test;

                             [AutoSpectreForm]
                             public class TestForm
                             {
                                  [TaskStep()]
                                  public void Hello()
                                  {
                                    
                                  }
                             }
                             """).Should().Contain("destination.Hello();").And.Contain("AnsiConsole.MarkupLine(\"Calling method [green]Hello[/]");
    }

    [Fact]
    public void TaskStepWithConsoleAndAsyncGeneratesExpected()
    {
        GetGeneratedOutput($$"""
                             using AutoSpectre;
                             using System.Collections.Generic;
                             using System.Threading.Tasks;
                             using Spectre.Console;

                             namespace Test;

                             [AutoSpectreForm]
                             public class TestForm
                             {
                                  [TaskStep()]
                                  public async Task HelloAsync(IAnsiConsole console)
                                  {
                                      await Task.Delay(5000);
                                  }
                             }
                             """).Should().Contain("destination.HelloAsync(");
    }

    [Fact]
    public void AsyncStepMethodWithoutParameter_GeneratesExpectedResult()
    {
        GetGeneratedOutput($$"""
                             using AutoSpectre;
                             using System.Collections.Generic;
                             using Spectre.Console;
                             using System.Threading.Tasks;
                             
                             namespace Test;
                             
                             [AutoSpectreForm]
                             public class Something
                             {
                                 [TaskStep]
                                 public async Task Intro()
                                 {
                                    await Task.Delay(5000);
                                 }

                                 [SelectPrompt(PageSize = 3, HighlightStyle = "grey")]
                                 public int SelectNumber { get; set; }

                                 public IEnumerable<int> SelectNumberSource()
                                 {
                                     return Enumerable.Range(1, 100);
                                 }
                             }
                            """).Should().Contain("await destination.Intro()").And.Contain("GetAsync");


    }

    [Fact]
    public void StatusTextAndUseStatusSetOnAsyncReturnExpected()
    {
        GetGeneratedOutput($$"""
                             using AutoSpectre;
                             using System.Collections.Generic;
                             using System.Threading.Tasks;

                             namespace Test;

                             [AutoSpectreForm]
                             public class TestForm
                             {
                                  [TaskStep(UseStatus = true, StatusText = "Hello")]
                                  public async Task Hello()
                                  {
                                        await Task.Delay(5000);
                                  }
                             }
                             """).Should().Contain("await AnsiConsole.Status.StartAsync(\"Hello\"");
    }



   
    
    public TaskStepGeneratorTests(ITestOutputHelper helper) : base(helper)
    {
    }
}