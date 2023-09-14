using FluentAssertions;
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
    public void InvalidChoicesStyleApplied()
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
    
}