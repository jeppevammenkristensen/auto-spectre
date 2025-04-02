using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests;

public class DefaultValueGeneratorTests : AutoSpectreGeneratorTestsBase
{
    public DefaultValueGeneratorTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public void DefaultValueCanPointToSelf()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(DefaultValueSource=nameof(Property))]
                         public string Property {get;set;} = "Jeppe"
                    }
                    """).OutputShouldContain(".DefaultValue(destination.Property)");
    }
    
    [Fact]
    public void DefaultStyleUsedOnConfirmation()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(DefaultValueStyle="yellow")]
                         public bool Property {get;set;} 
                    }
                    """).OutputShouldContain(""".DefaultValueStyle("yellow")""");
    }
    
    [Fact]
    public void ChoicesStyleUsedOnConfirmation()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(ChoicesStyle="yellow")]
                         public bool Property {get;set;}
                    }
                    """).OutputShouldContain(""".ChoicesStyle("yellow")""").OutputShouldContain("new ConfirmationPrompt");
    }
    
    [Theory]
    [InlineData("public string DefaultValueSource {get;set}", ".DefaultValue(destination.DefaultValueSource)")]
    [InlineData("public static string DefaultValueSource {get;set}", ".DefaultValue(Test.TestForm.DefaultValueSource)")]
    [InlineData("public static string DefaultValueSource() => string.Empty;", ".DefaultValue(Test.TestForm.DefaultValueSource())")]
    [InlineData("public string DefaultValueSource() => string.Empty;", ".DefaultValue(destination.DefaultValueSource())")]
    [InlineData("public const string DefaultValueSource = string.Empty;", "TestForm.DefaultValueSource")]
    [InlineData("public static readonly string DefaultValueSource = string.Empty;", "TestForm.DefaultValueSource")]
    [InlineData("public static string DefaultValueSource = string.Empty;", "TestForm.DefaultValueSource")]
    [InlineData("public string DefaultValueSource = string.Empty;", "destination.DefaultValueSource")]
    [InlineData("public readonly string DefaultValueSource = string.Empty;", "destination.DefaultValueSource")]
    public void DefaultValueValidSource(string source, string expected)
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(DefaultValueSource=nameof(DefaultValueSource))]
                         public string Property {get;set;} = "Jeppe"
                         
                         {{ source }}
                    }
                    """).OutputShouldContain(expected);
    }
    
    [Theory]
    [InlineData("private string DefaultValueSource {get;set}")]
    [InlineData("private static string DefaultValueSource {get;set}")]
    [InlineData("private static string DefaultValueSource() => string.Empty;")]
    [InlineData("public int DefaultValueSource() => 45")]
  
    public void DefaultValueIsInvalidReturnsDiagnostic(string source)
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(DefaultValueSource=nameof(DefaultValueSource))]
                         public string Property {get;set;} = "Jeppe"
                         
                         {{ source }}
                    }
                    """).ShouldHaveSourceGeneratorDiagnosticOnlyOnce(DiagnosticIds.Id0025_DefaultValueSource_NotFound);
    }
    
    [Theory]
    [InlineData("public string DefaultValueSource {get;set}", ".DefaultValue(destination.DefaultValueSource)")]
    [InlineData("public static string DefaultValueSource {get;set}", ".DefaultValue(Test.TestForm.DefaultValueSource)")]
    [InlineData("public static string DefaultValueSource() => string.Empty;", ".DefaultValue(Test.TestForm.DefaultValueSource())")]
    [InlineData("public string DefaultValueSource() => string.Empty;", ".DefaultValue(destination.DefaultValueSource())")]
    public void DefaultValueValidSourceTypeEnumerable(string source, string expected)
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(DefaultValueSource=nameof(DefaultValueSource))]
                         public List<string> Property {get;set;} = "Jeppe"
                         
                         {{ source }}
                    }
                    """).OutputShouldContain(expected);
    }

   
}
