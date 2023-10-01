﻿using FluentAssertions;
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

    [Fact]
    public void MissingSource_GeneratesCorrectDiagnostic()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [SelectPrompt()]
                         public string Source {get;set;}
                    }
                    """).ShouldHaveSourceGeneratorDiagnosticOnlyOnce(DiagnosticIds.Id0026_NoSelectionSource);
            ;
    }
    
    [Fact]
    public void MissingSource_NameMatchButInvalidFormGeneratesCorrectDiagnostic()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [SelectPrompt()]
                         public string Source {get;set;}
                         
                         public bool SourceSource() => true;
                    }
                    """).ShouldHaveSourceGeneratorDiagnosticOnlyOnce(DiagnosticIds.Id0026_NoSelectionSource);
    }
    
    [Fact]
    public void SourcePresent_NameMatchButInvalidFormGeneratesCorrectDiagnostic()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [SelectPrompt(Source = nameof(PropertySource))]
                         public string Property {get;set;}
                         
                         public bool PropertySource() => true;
                    }
                    """).ShouldHaveSourceGeneratorDiagnosticOnlyOnce(DiagnosticIds.Id0005_SelectionSourceNotFound);
    }
    
    [Theory]
    [InlineData("public static int[] SelectNumbersSource { get; set; }", "TestForm.SelectNumbersSource.")]
    [InlineData("public readonly int[] SelectNumbersSource;", "destination.SelectNumbersSource.")]
    [InlineData("public const int[] SelectNumbersSource;", "TestForm.SelectNumbersSource.")]
    [InlineData("public int[] SelectNumbersSource;", "destination.SelectNumbersSource.")]
    [InlineData("public static int[] SelectNumbersSource() => null;", "TestForm.SelectNumbersSource().")]
    [InlineData("public int[] SelectNumbersSource() => null;", "destination.SelectNumbersSource().")]
    public void SelectPromptWithValidVariation(string variation, string expected)
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                       [SelectPrompt(HighlightStyle="red")]
                       public int SelectNumbers {get;set;}
                    
                      {{variation}}
                    }
                    """).OutputShouldContain(expected);
    }
}