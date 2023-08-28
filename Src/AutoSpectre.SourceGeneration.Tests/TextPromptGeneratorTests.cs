using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests;

public class TextPromptGeneratorTests : AutoSpectreGeneratorTestsBase
{
    public TextPromptGeneratorTests(ITestOutputHelper helper) : base(helper)
    {

    }

    [Fact]
    public void PropertyWithDefaultValueLiteralSetGetsUsedAsDefault()
    {
        GetGeneratedOutput($$"""
                             using AutoSpectre;
                             using System.Collections.Generic;

                             namespace Test;
                             
                             [AutoSpectreForm]
                             public class TestForm
                             {
                                  [TextPrompt(DefaultValueStyle = "red")]
                                  public string WithADefault {get;set;} = "Hello";
                             }
                             """).Should().Contain($""".DefaultValue("Hello")""").And.Contain("DefaultValueStyle(\"red\")");
    }

    [Fact]
    public void PropertyWithPromptStyleRendersCorrectly()
    {
        GetGeneratedOutput($$"""
                             using AutoSpectre;
                             using System.Collections.Generic;

                             namespace Test;

                             [AutoSpectreForm]
                             public class TestForm
                             {
                                  [TextPrompt(PromptStyle="red"]
                                  public string Property {get;set;}
                             }
                             """).Should().Contain(""".PromptStyle("red")""");
    }
    
    [Fact]
    public void PropertyWithDefaultValueStringEmpty()
    {
        GetGeneratedOutput($$"""
                             using AutoSpectre;
                             using System.Collections.Generic;

                             namespace Test;

                             [AutoSpectreForm]
                             public class TestForm
                             {
                                  [TextPrompt]
                                  public string SomeProperty {get;set;} = string.Empty;
                             }
                             """).Should().Contain("DefaultValue(string.Empty)");
    }

    [Fact]
    public void PropertyExistingFormValidationSetsCorrectValidationMessage()
    {
        GetGeneratedOutput($$"""
                             using AutoSpectre;
                             using System.Collections.Generic;

                             namespace Test;
                             
                             [AutoSpectreForm]
                             public class ChildForm
                             {
                                [TextPrompt]
                                public string Title {get;set;}
                             }
                             
                             
                             [AutoSpectreForm]
                             public class TestForm
                             {
                                  [TextPrompt]
                                  public ChildForm Child {get;set;}
                                  
                                  public string? ChildValidator(ChildForm source)
                                  {
                                    return null;
                                  }
                             }
                             
                             
                             
                             """).Should().Contain("AnsiConsole.MarkupLineInterpolated($\"[red]{error}[/]\")");
    }
    
    [Fact]
    public void PropertyWithDefaultValueOtherSetGetsUsedAsDefault()
    {
        GetGeneratedOutput($$"""
                             using AutoSpectre;
                             using System.Collections.Generic;

                             namespace Test;

                             [AutoSpectreForm]
                             public class TestForm
                             {
                                  [TextPrompt]
                                  public string WithADefault {get;set;} = DefaultValue;
                                  
                                  public static readonly string DefaultValue = "Angry";
                             }
                             """).Should().Contain(".DefaultValue(TestForm.DefaultValue)");
    }


    
    
    [Fact]
    public void TextPromptWithSecretNoMaskGeneratesCorrectSecret()
    {
        GetGeneratedOutput("""
                           using AutoSpectre;
                           using System.Collections.Generic;

                           namespace Test;

                           [AutoSpectreForm]
                           public class TestForm
                           {
                               [TextPrompt(Secret=true)]
                               public string? Secret {get;set;}

                           }
                           """).Should().Contain(".Secret('*')");
    }

    [Fact]
    public void TextPromptWithSecretMaskNullGeneratesCorrectSecret()
    {
        GetGeneratedOutput("""
                           using AutoSpectre;
                           using System.Collections.Generic;

                           namespace Test;

                           [AutoSpectreForm]
                           public class TestForm
                           {
                               [TextPrompt(Secret=true, Mask=null)]
                               public string? Secret {get;set;}

                           }
                           """).Should().Contain(".Secret(null)");
    }
}