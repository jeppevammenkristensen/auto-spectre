﻿using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests;

public class ChoicesTests : AutoSpectreGeneratorTestsBase
{
    public ChoicesTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public void ChoicesAddedThroughConventionMethod()
    {
        GetOutput($$"""
                             using AutoSpectre;
                             using System.Collections.Generic;

                             namespace Test;

                             [AutoSpectreForm]
                             public class TestForm
                             {
                                   [TextPrompt]
                                   public string Test {get;set;}
                                   
                                   public string[] TestChoices() => new string[0];                            
                             }
                             """).OutputShouldContain(".AddChoices(destination.TestChoices())");
    }
    
    [Fact]
    public void ChoicesAddedThroughConventionProperty()
    {
        GetOutput($$"""
                             using AutoSpectre;
                             using System.Collections.Generic;

                             namespace Test;

                             [AutoSpectreForm]
                             public class TestForm
                             {
                                  [TextPrompt]
                                  public string Test {get;set;}
                                  
                                  public IEnumerable<string> TestChoices => null;
                             }
                             """).OutputShouldContain(".AddChoices(destination.TestChoices");
    }
    
    [Fact]
    public void ChoicesAddedThroughChoicesProperty()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(ChoicesSource = nameof(TheStrings)]
                         public string Test {get;set;}
                         
                         public IEnumerable<string> TheStrings => null;
                    }
                    """).OutputShouldContain(".AddChoices(destination.TheStrings");
    }
    
    [Fact]
    public void ChoiceStyleApplied()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(ChoicesSource = nameof(TheStrings), ChoicesStyle="Yellow on blue")]
                         public string Test {get;set;}
                         
                         public IEnumerable<string> TheStrings => null;
                    }
                    """).OutputShouldContain(".ChoicesStyle(\"Yellow on blue\")");
    }
    
    [Fact]
    public void InvalidChoicesTextApplied()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(ChoicesSource = nameof(TheStrings), ChoicesInvalidText = "Invalid text")]
                         public string Test {get;set;}
                         
                         public IEnumerable<string> TheStrings => null;
                    }
                    """).OutputShouldContain(".InvalidChoiceMessage(\"Invalid text\")");
    }
    
    [Theory]
    [InlineData("public static string[] TheStrings => new string[0]")]
    [InlineData("private string[] TheStrings => new string[0];")]
    [InlineData("public int[] TheStrings => new [] {45};")]
    [InlineData("public string[] TheStrings(string input) => new string[0];")]
    public void SourceMatchIsInvalidReturnsExpectedDiagnostic(string invalidSource)
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(ChoicesSource = nameof(TheStrings), ChoicesInvalidText = "Invalid text")]
                         public string Test {get;set;}
                         
                         {{invalidSource}}
                    }
                    """).ShouldHaveSourceGeneratorDiagnostic(DiagnosticIds.Id0023_ChoiceCandidate_NotValid);
    }
    
    [Theory]
    [InlineData("public static string[] TestChoices => new string[0];")]
    [InlineData("private List<string> TestChoices => new();")]
    [InlineData("public List<int> TestChoices => new() {45};")]
    [InlineData("public HashSet<string> TestChoices(int parameter) => new HasSet<string>();")]
    public void SourceMatchByConventionIsInvalidReturnsExpectedDiagnostic(string invalidSource)
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(ChoicesInvalidText = "Invalid text")]
                         public string Test {get;set;}
                         
                         {{invalidSource}}
                    }
                    """).ShouldHaveSourceGeneratorDiagnostic(DiagnosticIds.Id0023_ChoiceCandidate_NotValid);
    }
    
    [Theory]
    [InlineData("public string[] WrongSource => new string[0];")]
    [InlineData("public List<string> WrongSource() => new();")]
    public void NoSourceMatchByNameReturnsExpectedDiagnostic(string invalidSource)
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(ChoicesSource = "ExpectedSource", ChoicesInvalidText = "Invalid text")]
                         public string Test {get;set;}
                         
                         {{invalidSource}}
                    }
                    """).ShouldHaveSourceGeneratorDiagnostic(DiagnosticIds.Id0024_ChoiceCandidate_NotFound);
    }
    
}