using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests;

public class SelectPromptGeneratorTests : AutoSpectreGeneratorTestsBase
{
    public SelectPromptGeneratorTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public void SelectPromptWithMoreChoicesTextGeneratesExpectedResult()
    {
        GetGeneratedOutput($$"""
                             using AutoSpectre;
                             using System.Collections.Generic;

                             namespace Test;

                             [AutoSpectreForm]
                             public class TestForm
                             {
                                [SelectPrompt(MoreChoicesText = "more")]
                                public string SelectSomething {get;set;}
                                
                                public List<string> SelectSomethingSource => new List<string>();
                             }
                             """).Should().Contain(".MoreChoicesText(\"more\")");
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
                                public int SelectNumber {get;set;}
                                
                                public List<int> SelectNumberSource {get;set;}
                             }
                             """).Should().Contain(".PageSize(42)");
    }
    
    [Fact]
    public void SelectPromptWithWrapAroundGeneratesCorrectValue()
    {
        GetGeneratedOutput($$"""
                             using AutoSpectre;
                             using System.Collections.Generic;

                             namespace Test;

                             [AutoSpectreForm]
                             public class TestForm
                             {
                                [SelectPrompt(WrapAround=true)]
                                public int SelectNumber {get;set;}
                                
                                public List<int> SelectNumberSource {get;set;}
                             }
                             """).Should().Contain(".WrapAround(true)");
    }
}