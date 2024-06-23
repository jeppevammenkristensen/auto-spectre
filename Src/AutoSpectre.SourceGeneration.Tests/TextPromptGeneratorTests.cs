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
    public void TextPromptWithSpecialCharacter()
    {
        GetOutput("""
                  using AutoSpectre;
                  using System.Collections.Generic;

                  namespace Test;

                  [AutoSpectreForm]
                  public class TestForm
                  {
                      [TextPrompt(Title="\"test")]
                      public string Secret {get;set;}
                      
                     
                  }

                  

                  """).Output.Should().Contain("\"test");
    }

    [Fact]
    public void TextPromptWithReferenceToOtherThatHasNoEmptyPublicConstructorButItHasInitializer()
    {
        GetOutput("""
                  using AutoSpectre;
                  using System.Collections.Generic;

                  namespace Test;

                  [AutoSpectreForm]
                  public class TestForm
                  {
                      [TextPrompt(TypeInitializer=nameof(InitOther))]
                      public Other Secret {get;set;}
                      
                      public Other InitOther()
                      {
                        return new Other(1);
                      }

                  }

                  [AutoSpectreForm]
                  public class Other
                  {
                       public Other(int input)
                       {
                           
                       }
                  }

                  """).HasNoSourceGeneratorDiagnosticWith(DiagnosticIds.Id0020_InitializerNeeded).Output.Should().Contain("var item = destination.InitOther();");
    }

    [Fact]
    public void TextPromptWithReferenceToOtherNoEmptyConstructorButInitializerThroughConvention()
    {
        GetOutput("""
                  using AutoSpectre;
                  using System.Collections.Generic;

                  namespace Test;

                  [AutoSpectreForm]
                  public class TestForm
                  {
                      [TextPrompt()]
                      public Other Secret {get;set;}
                      
                      public Other InitOther()
                      {
                          return new Other("Test");
                      }

                  }

                  [AutoSpectreForm]
                  public class Other
                  {
                       public Other(string name)
                       {
                           
                       }
                  }

                  """)
            .HasNoSourceGeneratorDiagnosticWith(DiagnosticIds.Id0020_InitializerNeeded)
            .OutputShouldContain("var item = destination.InitOther();");
    }
    
    [Fact]
    public void TextPromptWithReferenceToOtherThatHasEmptyPublicConstructor()
    {
        GetOutput("""
                  using AutoSpectre;
                  using System.Collections.Generic;

                  namespace Test;

                  [AutoSpectreForm]
                  public class TestForm
                  {
                      [TextPrompt(Secret=true)]
                      public Other Secret {get;set;}

                  }

                  [AutoSpectreForm]
                  public class Other
                  {
                       public Other()
                       {
                           
                       }
                  }

                  """).HasNoSourceGeneratorDiagnosticWith(DiagnosticIds.Id0020_InitializerNeeded);
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