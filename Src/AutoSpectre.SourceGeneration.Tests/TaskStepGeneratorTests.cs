﻿using FluentAssertions;
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
                                  public TestForm(int dfd)
                                  {
                                  
                                  }
                             
                                  [TaskStep()]
                                  public void Hello()
                                  {
                                    
                                  }
                             }
                             """).Should().Contain("form.Hello();");

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
                             """).Should().Contain("form.HelloAsync(");
    }

    [Fact]
    public void TaskStepWithConditionCodeIsWrappedWithCondition()
    {
        GetGeneratedOutput($$"""
                             using AutoSpectre;
                             using System.Collections.Generic;
                             using System.Threading.Tasks;
                             
                             
                             namespace Test;

                             [AutoSpectreForm]
                             public class TestForm
                             {
                                [TextPrompt]
                                public bool Test {get;set;}
                             
                                [TaskStep(Condition = nameof(Test))]
                                public async Task HelloAsync()
                                {
                                    await Task.Delay(5000);
                                }
                             }
                             """).Should().Contain("if (form.Test == true)");
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
                             """).Should().Contain("await form.Intro()").And.Contain("PromptAsync");
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
                             """).Should()
            .Contain("await AnsiConsole.Status().StartAsync(\"Hello\"");
    }

    [Fact]
    public void TitleNotEmtpyIsDisplayed()
    {
        GetGeneratedOutput($$"""
                             using AutoSpectre;
                             using System.Collections.Generic;

                             namespace Test;

                             [AutoSpectreForm]
                             public class TestForm
                             {
                                   [TaskStep(Title = "Calling mister raider")]
                                   public void CustomStep() 
                                   {
                                   }
                             }
                             """).Should().Contain("AnsiConsole.MarkupLine(\"Calling mister raider\");");
    }

    [Fact]
    public void SpinnerTypeSetGeneratesCorrect()
    {
        GetGeneratedOutput($$"""
                             using AutoSpectre;
                             using System.Collections.Generic;
                             using System.Threading.Tasks;

                             namespace Test;

                             [AutoSpectreForm]
                             public class TestForm
                             {
                                  [TaskStep(UseStatus = true, StatusText = "Hello", SpinnerType = SpinnerKnownTypes.Balloon)]
                                  public async Task Hello()
                                  {
                                        await Task.Delay(5000);
                                  }
                             }
                             """).Should().Contain(".Spinner(Spinner.Known.Balloon).");
    }


    public TaskStepGeneratorTests(ITestOutputHelper helper) : base(helper)
    {
    }
}