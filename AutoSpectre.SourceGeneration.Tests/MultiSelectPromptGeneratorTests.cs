using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests;

public class MultiSelectPromptGeneratorTests : AutoSpectreGeneratorTestsBase
{
    public MultiSelectPromptGeneratorTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public void SelectPromptWithPageSizeGeneratesCorrectValue()
    {
        GetGeneratedOutput($$"""
                             using AutoSpectre;
                             using System.Collections.Generic;

                             namespace Test;

                             [AutoSpectreForm]
                             public class TestForm
                             {
                                  [SelectPrompt(PageSize=42)]
                                  public int[] SelectNumbers {get;set;}
                                  
                                  public List<int> SelectNumbersSource {get;set;}
                             }
                             """).Should().Contain(".PageSize(42)");
    }

    [Fact]
    public void SelectPromptWithWrapAroundGeneratesCorrectPart()
    {
        GetGeneratedOutput($$"""
                             using AutoSpectre;
                             using System.Collections.Generic;

                             namespace Test;

                             [AutoSpectreForm]
                             public class TestForm
                             {
                                  [SelectPrompt(WrapAround=true)]
                                  public int[] SelectNumbers {get;set;}
                                  
                                  public List<int> SelectNumbersSource {get;set;}
                             }
                             """).Should().Contain(".WrapAround(true)");
    }
}